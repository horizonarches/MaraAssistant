#pragma warning disable OPENAI001
using OpenAI.Audio;
using Plugin.Maui.Audio;
using System.ClientModel;
#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace PatientCareChatbotPortal.Services;

public sealed class AudioService : IDisposable
{
    private readonly IAudioManager _audioManager;
    private AsyncAudioPlayer? _speechPlayer;
    private MemoryStream? _speechBuffer;

#if WINDOWS
    private MediaPlayer? _backgroundPlayer;
#endif

    private AppStateService _state;

    public AudioService(IAudioManager audioManager, AppStateService state)
    {
        _audioManager = audioManager;
        _state = state;
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        await TextToSpeech.Default.SpeakAsync(text, null, cancellationToken);
    }

    public Task PlayEffectAsync(string? effectTag)
    {
        var tag = (effectTag ?? string.Empty).Trim().ToLowerInvariant();
#if WINDOWS
        switch (tag)
        {
            case "alert":
                Console.Beep(950, 180);
                break;
            case "success":
                //Console.Beep(1200, 120);
                break;
            default:
                //Console.Beep(800, 80);
                break;
        }
#endif
        return Task.CompletedTask;
    }

    public Task PlayBackgroundAudioAsync(string? backgroundTag)
    {
        var tag = (backgroundTag ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(tag) || tag.ToLowerInvariant() == "none" || tag == "off" || tag == _state.CurrentBackgroundAudio)
        {
            StopBackgroundAudio();
            return Task.CompletedTask;
        }

#if WINDOWS
        try
        {
            _backgroundPlayer ??= new MediaPlayer
            {
                IsLoopingEnabled = true,
                Volume = 0.1
            };

            var preferred = Path.Combine(AppContext.BaseDirectory, "Resources", "Raw", tag);
            var fallback = Path.Combine(AppContext.BaseDirectory, tag);
            var audioPath = File.Exists(preferred) ? preferred : fallback;
            if (!File.Exists(audioPath))
            {
                return Task.CompletedTask;
            }

            _backgroundPlayer.Source = MediaSource.CreateFromUri(new Uri(audioPath));
            _backgroundPlayer.Play();
            _state.CurrentBackgroundAudio = tag; 
        }
        catch
        {
            // Ignore background audio startup failures in this front-end scaffold.
        }
#endif

        return Task.CompletedTask;
    }

    public async Task PlayGeneratedSpeechAsync(AsyncCollectionResult<StreamingSpeechUpdate> updates, CancellationToken cancellationToken = default)
    {
        _speechPlayer?.Stop();
        _speechPlayer?.Dispose();
        _speechBuffer?.Dispose();

        _speechBuffer = new MemoryStream();

        await foreach (var update in updates.WithCancellation(cancellationToken))
        {
            if (update is StreamingSpeechAudioDeltaUpdate delta)
            {
                var bytes = delta.AudioBytes.ToArray();
                await _speechBuffer.WriteAsync(bytes, cancellationToken);
            }
        }

        if (_speechBuffer.Length == 0)
        {
            return;
        }

        _speechBuffer.Position = 0;
        _speechPlayer = _audioManager.CreateAsyncPlayer(_speechBuffer);
        _speechPlayer.Volume = 1.0;
        await _speechPlayer.PlayAsync(cancellationToken);
    }

    public void StopBackgroundAudio()
    {
#if WINDOWS
        _backgroundPlayer?.Pause();
#endif
    }

    public void SetBackgroundVolume(double volume)
    {
#if WINDOWS
        var clamped = Math.Clamp(volume, 0, 1);
        if (_backgroundPlayer is not null)
        {
            _backgroundPlayer.Volume = clamped;
        }
#endif
    }

    public void Dispose()
    {
        _speechPlayer?.Stop();
        _speechPlayer?.Dispose();
        _speechPlayer = null;

        _speechBuffer?.Dispose();
        _speechBuffer = null;

#if WINDOWS
        _backgroundPlayer?.Dispose();
        _backgroundPlayer = null;
#endif
    }
}
#pragma warning restore OPENAI001
