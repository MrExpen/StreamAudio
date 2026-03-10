using System.Collections.Frozen;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using StreamAudio.Core.Enums;
using StreamAudio.Core.Interfaces;
using StreamAudio.Core.Models;

namespace StreamAudio.Engine.WASAPI;

public sealed class WasapiAudioEngine : IAudioEngine
{
    private FrozenDictionary<string, MMDevice> _inputDevices = null!;
    private FrozenDictionary<string, MMDevice> _outputDevices = null!;

    public void Initialize()
    {
        using var enumerator = new MMDeviceEnumerator();

        _inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .ToFrozenDictionary(device => device.ID, device => device);

        _outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
            .ToFrozenDictionary(device => device.ID, device => device);
    }

    public IEnumerable<AudioDevice> GetDevices(AudioDirection direction)
    {
        return direction switch
        {
            AudioDirection.Unknown => throw new InvalidOperationException(),
            AudioDirection.Input => _inputDevices.Values.Select(Map),
            AudioDirection.Output => _outputDevices.Values.Select(Map),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public IAudioStream StartStreaming(string inputId, string outputId)
    {
        var capture = GetCaptureDevice(inputId);
        var output = GetOutputDevice(outputId);

        var bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat)
        {
            DiscardOnBufferOverflow = true
        };

        capture.DataAvailable += (s, a) => { bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded); };

        var provider = new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider());

        output.Init(provider);

        output.Play();
        capture.StartRecording();

        return new StreamWrapper(capture, output, provider);
    }

    private static AudioDevice Map(MMDevice device)
    {
        return new AudioDevice
        {
            Id = device.ID,
            Direction = device.DataFlow switch
            {
                DataFlow.Render => AudioDirection.Output,
                DataFlow.Capture => AudioDirection.Input,
                DataFlow.All => AudioDirection.Unknown,
                _ => throw new ArgumentOutOfRangeException()
            },
            Name = device.DataFlow == DataFlow.Capture ? device.FriendlyName : $"{device.FriendlyName} (loopback)",
        };
    }

    private WasapiCapture GetCaptureDevice(string deviceId)
    {
        if (_inputDevices.TryGetValue(deviceId, out var inputDevice))
        {
            return new WasapiCapture(inputDevice);
        }

        if (_outputDevices.TryGetValue(deviceId, out var outputDevice))
        {
            return new WasapiLoopbackCapture(outputDevice);
        }

        throw new InvalidOperationException();
    }

    private WasapiOut GetOutputDevice(string deviceId)
    {
        if (_outputDevices.TryGetValue(deviceId, out var outputDevice))
        {
            return new WasapiOut(outputDevice, AudioClientShareMode.Shared, true, 0 /* TODO */);
        }

        throw new InvalidOperationException();
    }

    public void Dispose()
    {
    }
}