using System.Diagnostics.Contracts;
using Spectre.Console;
using StreamAudio.Core.Enums;
using StreamAudio.Core.Interfaces;
using StreamAudio.Core.Models;
using StreamAudio.Engine.WASAPI;

IAudioEngine engine = new WasapiAudioEngine();

AnsiConsole.Status().Start("Initializing audio engine", _ => engine.Initialize());

var input = GetInputDevice(engine);
var output = GetOutputDevice(engine);
var volume = GetVolume();

using var stream = engine.StartStreaming(input.Id, output.Id);

stream.Volume = volume / 100f;

await AnsiConsole.Status().StartAsync("Streaming...", async _ => { await Task.Delay(Timeout.Infinite); });

return;

[Pure]
static AudioDevice GetInputDevice(IAudioEngine engine)
{
    var input = engine.GetDevices(AudioDirection.Input);
    var output = engine.GetDevices(AudioDirection.Output);

    var all = input.Concat(output).ToArray();

    var prompt = new SelectionPrompt<AudioDevice>()
        .Title("Select [bold]input[/] device:")
        .AddChoices(all)
        .UseConverter(device => device.Name);

    return AnsiConsole.Prompt(prompt);
}

static AudioDevice GetOutputDevice(IAudioEngine engine)
{
    var output = engine.GetDevices(AudioDirection.Output).ToArray();

    var prompt = new SelectionPrompt<AudioDevice>()
        .Title("Select [bold]output[/] device:")
        .AddChoices(output)
        .UseConverter(device => device.Name);

    return AnsiConsole.Prompt(prompt);
}

static float GetVolume()
{
    var prompt = new TextPrompt<float>("Enter volume:")
        .HideChoices()
        .Validate(f => f is >= 0 and <= 10)
        .DefaultValue(100);

    return AnsiConsole.Prompt(prompt);
}