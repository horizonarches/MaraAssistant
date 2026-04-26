using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Pages;

public partial class AdminPage : ContentPage
{
    private readonly AppStateService _state;
    private readonly NavigationService _navigationService;

    public AdminPage(AppStateService state, NavigationService navigationService)
    {
        InitializeComponent();
        _state = state;
        _navigationService = navigationService;
        _state.StateChanged += OnStateChanged;
        BindState();
    }

    private async void OnOpenModalClicked(object sender, EventArgs e)
    {
        await _navigationService.OpenModalAsync<AdminConfigModalPage>();
    }

    private void OnStateChanged() => MainThread.BeginInvokeOnMainThread(BindState);

    private void BindState()
    {
        var patient = _state.CurrentPatientCondition;
        var patientSummary = _state.CurrentPatientSummary;
        var room = _state.CurrentRoomCondition;

        PainLevelLabel.Text = patient._pain_level.ToString("0.##", CultureInfo.InvariantCulture);
        StressLevelLabel.Text = patient._stress_level.ToString("0.##", CultureInfo.InvariantCulture);
        SleepQualityLabel.Text = patient._sleep_quality.ToString("0.##", CultureInfo.InvariantCulture);

        LightLabel.Text = room._light.ToString("0.##", CultureInfo.InvariantCulture);

        var temperatureText = string.Equals(room._temperatureUnit, "Celsius", StringComparison.OrdinalIgnoreCase)
            ? $"{room._temperature_c.ToString("0.##", CultureInfo.InvariantCulture)} C"
            : $"{room._temperature_f.ToString("0.##", CultureInfo.InvariantCulture)} F";
        TemperatureLabel.Text = temperatureText;

        BedInclineLabel.Text = $"{room._bedInclineDegrees.ToString("0.##", CultureInfo.InvariantCulture)} degrees";
        NotesLabel.Text = $"{patientSummary._clinicalNotes.Length} characters loaded";
        RecordingLabel.Text = _state.RecordingActive ? "Active" : "Inactive";
    }

    protected override void OnDisappearing()
    {
        _state.StateChanged -= OnStateChanged;
        base.OnDisappearing();
    }
}
