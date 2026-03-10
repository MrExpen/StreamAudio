using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using StreamAudio.Core.Interfaces;

namespace StreamAudio.Engine.WASAPI;

internal class StreamWrapper : IAudioStream
{
    private readonly WasapiCapture _capture;
    private readonly WasapiOut _out;
    private readonly VolumeSampleProvider _volumeSampleProvider;

    public StreamWrapper(WasapiCapture capture, WasapiOut @out, VolumeSampleProvider volumeSampleProvider)
    {
        _capture = capture;
        _out = @out;
        _volumeSampleProvider = volumeSampleProvider;
    }

    public void Dispose()
    {
        _capture.Dispose();
        _out.Dispose();
    }

    public float Volume
    {
        get => _volumeSampleProvider.Volume;
        set => _volumeSampleProvider.Volume = value;
    }
}