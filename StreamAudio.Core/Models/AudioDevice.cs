using StreamAudio.Core.Enums;

namespace StreamAudio.Core.Models;

public class AudioDevice
{
    public required string Id { get; set; }
    public AudioDirection Direction { get; set; }
    public required string Name { get; set; }
}