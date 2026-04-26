using OpenAI.Audio;
using System.Text.Json;
using PatientCareChatbotPortal.Models;
using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Pages;

public partial class UserPage : ContentPage
{
    private readonly AppStateService _state;
    private readonly ChatbotService _chatbot;
    private readonly ThemeService _themeService;
    private readonly AudioService _audioService;
    private readonly SpeechRecognitionService _speechService;
    private bool _voiceEnabled = true;
    private bool _musicEnabled = true;

    public UserPage(
        AppStateService state,
        ChatbotService chatbot,
        ThemeService themeService,
        AudioService audioService,
        SpeechRecognitionService speechService)
    {
        InitializeComponent();

        _state = state;
        _chatbot = chatbot;
        _themeService = themeService;
        _audioService = audioService;
        _speechService = speechService;

        _state.StateChanged += OnStateChanged;
        UpdateMicButtonState();
        RefreshGauges();
        Loaded += OnLoaded;
    }

    private void OnStateChanged() => MainThread.BeginInvokeOnMainThread(() =>
    {
        RefreshGauges();
        UpdateMicButtonState();
    });

    private void RefreshGauges()
    {
        double tempF = Clamp(_state.CurrentRoomCondition._temperature_f, 50, 90);
        TemperatureRuler.Value = tempF;
        TemperatureValueLabel.Text = $"{tempF:0.#} F";

        double light = Clamp(_state.CurrentRoomCondition._light, 0, 100);
        LightingRuler.Value = light;
        LightingValueLabel.Text = $"{light:0.#}";

        double incline = Clamp(_state.CurrentRoomCondition._bedInclineDegrees, 0, 60);
        BedInclineRuler.Value = incline;
        BedInclineValueLabel.Text = $"{incline:0.#} deg";

        double pain = Clamp(_state.CurrentPatientCondition._pain_level, 0, 10);
        PainLevelValueLabel.Text = $"{pain:0.#} / 10";

        double stress = Clamp(_state.CurrentPatientCondition._stress_level, 0, 10);
        StressLevelValueLabel.Text = $"{stress:0.#} / 10";

        double sleepDifficulty = Clamp(_state.CurrentPatientCondition._sleep_quality, 0, 10);
        SleepDifficultyValueLabel.Text = $"{sleepDifficulty:0.#} / 10";
    }

    private static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));

    private void UpdateMicButtonState()
    {
        if (MicButton is null)
        {
            return;
        }

        MicButton.BackgroundColor = _state.RecordingActive ? Color.FromArgb("#C62828") : Color.FromArgb("#2E7D32");
    }

    private async void OnMicClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_state.RecordingActive) {
                _state.RecordingActive = true;
                UpdateMicButtonState();
                var heard = await _speechService.BeginRecordingAsync();
                if (!string.IsNullOrWhiteSpace(heard))
                {
                    PromptEntry.Text = heard;
                }
                else
                {
                    await DisplayAlert("Microphone", "No speech recognized.", "OK");
                }
            } else {
                _state.RecordingActive = false;
                UpdateMicButtonState();
                AudioTranscription transcription = await _speechService.StopRecordingAsync();

                AppendChat("User", transcription.Text);

                var payloadJson = await _chatbot.GetTaggedResponseAsync(transcription.Text, _state);

                var payload = DeserializePayload(payloadJson);

                AppendChat("Mara", payload.Response);
                AppendChatWithChanges(payload);

                _state.CurrentPatientCondition = payload.PatientCondition;
                _state.CurrentRoomCondition = payload.RoomCondition;
                _state.CurrentPatientSummary._introducedSelf = true;
                _state.NotifyStateChanged();
                PromptEntry.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Microphone error", ex.Message, "OK");
            UpdateMicButtonState();
        }
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (!_state.CurrentPatientSummary._introducedSelf)
        {
            await InitializeMara();
        }
    }

    private async Task InitializeMara()
    {
        PromptEntry.Text = "Hello Mara, this is the system stating the patient is ready for the session.  Go ahead and start!  Thank you!";
        AppendChat("System", PromptEntry.Text);

        var payloadJson = await _chatbot.GetTaggedResponseAsync(PromptEntry.Text, _state);
        var payload = DeserializePayload(payloadJson);

        AppendChat("Mara", payload.Response);
        AppendChatWithChanges(payload);

        _state.CurrentPatientCondition = payload.PatientCondition;
        _state.CurrentRoomCondition = payload.RoomCondition;
        _state.CurrentPatientSummary._introducedSelf = true;
        _state.NotifyStateChanged();

        AppendChat("System", "Session ready. You can type or use the microphone button.");
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        var prompt = PromptEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        PromptEntry.Text = string.Empty;
        AppendChat("User", prompt);

        var payloadJson = await _chatbot.GetTaggedResponseAsync(prompt, _state);
        var payload = DeserializePayload(payloadJson);

        AppendChat("Mara", payload.Response);
        AppendChatWithChanges(payload);

        _state.CurrentPatientCondition = payload.PatientCondition;
        _state.CurrentRoomCondition = payload.RoomCondition;
        _state.CurrentPatientSummary._introducedSelf = true;
        _state.NotifyStateChanged();
    }

    private void AppendChatWithChanges(ChatbotPayload payload)
    {
        if (_state.CurrentRoomCondition._light != payload.RoomCondition._light)
        {
            AppendChat("System", $"Lighting changed to {payload.RoomCondition._light:0.#}");
        }
        if (_state.CurrentRoomCondition._temperature_f != payload.RoomCondition._temperature_f)
        {
            AppendChat("System", $"Temperature changed to {payload.RoomCondition._temperature_f:0.#}");
        }
        if (_state.CurrentRoomCondition._bedInclineDegrees != payload.RoomCondition._bedInclineDegrees)
        {
            AppendChat("System", $"Bed incline changed to {payload.RoomCondition._bedInclineDegrees:0.#} degrees");
        }
        if (_state.CurrentRoomCondition._sound != payload.RoomCondition._sound)
        {
            AppendChat("System", $"Background audio changed to {payload.RoomCondition._sound}");
        }
        if (_state.CurrentPatientCondition._pain_level != payload.PatientCondition._pain_level)
        {
            AppendChat("System", $"Patient pain level changed to {payload.PatientCondition._pain_level:0.#}");
        }
        if (_state.CurrentPatientCondition._stress_level != payload.PatientCondition._stress_level)
        {
            AppendChat("System", $"Patient stress level changed to {payload.PatientCondition._stress_level:0.#}");
        }
        if (_state.CurrentPatientCondition._sleep_quality != payload.PatientCondition._sleep_quality)
        {
            AppendChat("System", $"Patient sleep quality changed to {payload.PatientCondition._sleep_quality:0.#}");
        }
    }

    private void OnVoiceToggled(object sender, ToggledEventArgs e)
    {
        _voiceEnabled = e.Value;
    }

    private void OnMusicToggled(object sender, ToggledEventArgs e)
    {
        _musicEnabled = e.Value;
        if (!_musicEnabled)
        {
            _audioService.StopBackgroundAudio();
        }
    }

    private void OnMusicVolumeChanged(object sender, ValueChangedEventArgs e)
    {
        _audioService.SetBackgroundVolume(e.NewValue);
    }

    private void OnStopMusicClicked(object sender, EventArgs e)
    {
        _audioService.StopBackgroundAudio();
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        var transcript = ChatHistoryLabel.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(transcript))
        {
            await DisplayAlert("Export", "Chat history is empty.", "OK");
            return;
        }

        try
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = $"patient-care-transcript-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            var path = Path.Combine(documents, fileName);

            await File.WriteAllTextAsync(path, transcript);
            AppendChat("System", $"Transcript exported: {path}");
            await DisplayAlert("Export complete", $"Saved transcript to:{Environment.NewLine}{path}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export failed", ex.Message, "OK");
        }
    }

    private static ChatbotPayload DeserializePayload(string payloadJson)
    {
        try
        {
            return JsonSerializer.Deserialize<ChatbotPayload>(payloadJson) ?? new ChatbotPayload
            {
                Response = payloadJson,
                //Theme = "default"
            };
        }
        catch
        {
            return new ChatbotPayload
            {
                Response = payloadJson,
                //Theme = "default"
            };
        }
    }

    private void AppendChat(string speaker, string message)
    {
        var line = $"[{DateTime.Now:T}] {speaker}: {message}{Environment.NewLine}";
        ChatHistoryLabel.Text += line;
        MainThread.BeginInvokeOnMainThread(async () => await ChatHistoryScroll.ScrollToAsync(ChatHistoryLabel, ScrollToPosition.End, false));
    }

    protected override void OnDisappearing()
    {
        _audioService.StopBackgroundAudio();
        _state.StateChanged -= OnStateChanged;
        base.OnDisappearing();
    }
}
