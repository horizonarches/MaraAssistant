using DocumentFormat.OpenXml.Presentation;
using OpenAI.Audio;
#if WINDOWS
using Plugin.Maui.Audio;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
#endif

namespace PatientCareChatbotPortal.Services;

public sealed class SpeechRecognitionService
{
    public readonly IAudioManager AudioManager;
    public readonly IAudioRecorder AudioRecorder;
    public readonly TranscriptionAudioClient MyAudioClient;

    public SpeechRecognitionService(IAudioManager audioManager, TranscriptionAudioClient audioClient)
    {
        AudioManager = audioManager;
        AudioRecorder = audioManager.CreateRecorder();
        MyAudioClient = audioClient;
    }

    public async Task<string> BeginRecordingAsync(CancellationToken cancellationToken = default)
    {
        await AudioRecorder.StartAsync("C:\\Users\\horiz\\Documents\\recordingtemp.wav");
        return "Recording started.";
    }

    public async Task<AudioTranscription> StopRecordingAsync(CancellationToken cancellationToken = default)
    {
        IAudioSource audioSource = await AudioRecorder.StopAsync();
        //var audioPlayer = this.AudioManager.CreateAsyncPlayer(((FileAudioSource)audioSource).GetAudioStream());
        //await audioPlayer.PlayAsync(CancellationToken.None);
        
        AudioTranscriptionOptions options = new()
        {
            ResponseFormat = AudioTranscriptionFormat.Simple,
            TimestampGranularities = AudioTimestampGranularities.Word | AudioTimestampGranularities.Segment,
        };

        AudioTranscription transcription = await MyAudioClient.Client.TranscribeAudioAsync(((FileAudioSource)audioSource).GetFilePath(), options);

        return transcription;
    }

    public async Task<string?> CaptureOnceAsync(CancellationToken cancellationToken = default)
    {
#if WINDOWS
        var recognizer = new SpeechRecognizer(new Language("en-US"));
        var compilationResult = await recognizer.CompileConstraintsAsync();
        if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
        {
            return null;
        }

        var recognitionResult = await recognizer.RecognizeAsync();
        return recognitionResult.Status == SpeechRecognitionResultStatus.Success
            ? recognitionResult.Text
            : null;
#else
        await Task.CompletedTask;
        return null;
#endif
    }

    
}
