namespace StreamAudio.Core.Interfaces;

public interface IAudioStream : IDisposable
{
    public float Volume { get; set; }
}