using System.Diagnostics.Contracts;
using StreamAudio.Core.Enums;
using StreamAudio.Core.Interfaces;
using StreamAudio.Core.Models;
using StreamAudio.Engine.WASAPI;

IAudioEngine engine = new WasapiAudioEngine();

engine.Initialize();

var input = GetInputDevice(engine);
var output = GetOutputDevice(engine);

using var stream = engine.StartStreaming(input.Id, output.Id);

stream.Volume = GetVolume();

await Task.Delay(Timeout.Infinite);
return;

[Pure]
static AudioDevice GetInputDevice(IAudioEngine engine)
{
    var input = engine.GetDevices(AudioDirection.Input);
    var output = engine.GetDevices(AudioDirection.Output);

    var all = input.Concat(output).ToArray();

    Console.WriteLine("Выберите input:");

    for (int i = 0; i < all.Length; i++)
    {
        Console.WriteLine($"{i,2}: {all[i].Name}");
    }

    return all[int.Parse(Console.ReadLine()!)];
}

static AudioDevice GetOutputDevice(IAudioEngine engine)
{
    var output = engine.GetDevices(AudioDirection.Output).ToArray();

    Console.WriteLine("Выберите output:");
    for (int i = 0; i < output.Length; i++)
    {
        Console.WriteLine($"{i,2}: {output[i].Name}");
    }

    return output[int.Parse(Console.ReadLine()!)];
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