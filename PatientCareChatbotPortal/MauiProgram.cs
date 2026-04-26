using OpenAI.Audio;
using OpenAI.Responses;
using PatientCareChatbotPortal.Services;
using Plugin.Maui.Audio;

namespace PatientCareChatbotPortal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<AppStateService>();
        builder.Services.AddSingleton<NavigationService>();
        builder.Services.AddSingleton<ChatbotService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<SpeechRecognitionService>();
        builder.Services.AddSingleton<BuildInfoService>();
        builder.AddAudio();

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");

        builder.Services.AddSingleton(new TranscriptionAudioClient(apiKey));
        builder.Services.AddSingleton(new SpeechGenerationAudioClient(apiKey));

        #pragma warning disable OPENAI001
        builder.Services.AddSingleton<ResponsesClient>(serviceProvider => {
            return new ResponsesClient(apiKey);
        });
        #pragma warning restore OPENAI001
        return builder.Build();
    }
}
