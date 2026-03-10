using System.Diagnostics.Contracts;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

var enumerator = new MMDeviceEnumerator();
var capture = GetCaptureDevice(enumerator);
var output = new WasapiOut(GetOutputDevice(enumerator), AudioClientShareMode.Shared, true, GetLatency());
var volume = GetVolume();
var bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat)
{
    DiscardOnBufferOverflow = true
};

capture.DataAvailable += (s, a) => { bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded); };

if (volume == 100)
{
    output.Init(bufferedWaveProvider);
}
else
{
    output.Init(new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider())
    {
        Volume = volume / 100f
    });
}


Console.WriteLine("Стриминг микрофона в динамики...");

output.Play();
capture.StartRecording();

await Task.Delay(Timeout.Infinite);
return;

[Pure]
static WasapiCapture GetCaptureDevice(MMDeviceEnumerator enumerator)
{
    var input = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
    var output = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

    var all = input.Concat(output).ToArray();

    Console.WriteLine("Выберите input:");
    for (int i = 0; i < all.Length; i++)
    {
        Console.WriteLine($"{i,2}: {all[i].FriendlyName}");
    }

    var choice = all[int.Parse(Console.ReadLine()!)];

    return choice.DataFlow == DataFlow.Capture ? new WasapiCapture(choice) : new WasapiLoopbackCapture(choice);
}

static MMDevice GetOutputDevice(MMDeviceEnumerator enumerator)
{
    var output = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();

    Console.WriteLine("Выберите output:");
    for (int i = 0; i < output.Length; i++)
    {
        Console.WriteLine($"{i,2}: {output[i].FriendlyName}");
    }

    var choice = output[int.Parse(Console.ReadLine()!)];

    return choice;
}

static int GetLatency()
{
    Console.Write("Введите желаемую задержку (По умолчанию 20): ");

    return int.TryParse(Console.ReadLine(), out var result) ? result : 20;
}

static int GetVolume()
{
    Console.Write("Введите громкость вывода звука (Пу умолчанию 100%): ");

    return int.TryParse(Console.ReadLine(), out var result) ? result : 100;
}