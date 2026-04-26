using OpenAI.Audio;

namespace PatientCareChatbotPortal.Services;

public sealed class TranscriptionAudioClient
{
    public AudioClient Client { get; }

    public TranscriptionAudioClient(string apiKey)
    {
        Client = new AudioClient("gpt-4o-mini-transcribe", apiKey);
    }
}

public sealed class SpeechGenerationAudioClient
{
    public AudioClient Client { get; }

    public SpeechGenerationAudioClient(string apiKey)
    {
        Client = new AudioClient("gpt-4o-mini-tts", apiKey);
    }
}