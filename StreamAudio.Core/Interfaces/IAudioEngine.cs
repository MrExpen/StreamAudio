using StreamAudio.Core.Enums;
using StreamAudio.Core.Models;

namespace StreamAudio.Core.Interfaces;

public interface IAudioEngine : IDisposable
{
    public void Initialize();

    public IEnumerable<AudioDevice> GetDevices(AudioDirection direction);

    public IAudioStream StartStreaming(string inputId, string outputId);
}