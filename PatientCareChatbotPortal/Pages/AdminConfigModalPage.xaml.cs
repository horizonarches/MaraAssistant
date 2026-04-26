using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using PatientCareChatbotPortal.Models;
using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Pages;

public partial class AdminConfigModalPage : ContentPage
{
    private readonly AppStateService _state;

    public AdminConfigModalPage(AppStateService state)
    {
        InitializeComponent();
        _state = state;
        _state.CurrentPatientSummary._introducedSelf = false;
        _state.CurrentRoomCondition._sound = "NONE";
        _state.CurrentPatientSummary._clinicalNotes = string.Empty;

        PopulatePatientPickers();
        BindFromState();
    }

    private void PopulatePatientPickers()
    {
        foreach (var title in AppStateService.PatientTitleOptions)
        {
            PatientTitlePicker.Items.Add(title);
        }

        foreach (var race in AppStateService.PatientRaceOptions)
        {
            PatientRacePicker.Items.Add(race);
        }

        foreach (var gender in AppStateService.PatientGenderAssignedAtBirthOptions)
        {
            PatientGenderPicker.Items.Add(gender);
        }

        foreach (var religion in AppStateService.PatientReligionOptions)
        {
            PatientReligionPicker.Items.Add(religion);
        }

        foreach (var nationality in AppStateService.PatientNationalityOptions)
        {
            PatientNationalityPicker.Items.Add(nationality);
        }

        for (var level = 0; level <= 10; level++)
        {
            var levelText = level.ToString(CultureInfo.InvariantCulture);
            CurrentPainLevelPicker.Items.Add(levelText);
            CurrentStressLevelPicker.Items.Add(levelText);
            CurrentDifficultySleepingPicker.Items.Add(levelText);
        }
    }

    private void BindFromState()
    {
        var patientSummary = _state.CurrentPatientSummary;
        var patientCondition = _state.CurrentPatientCondition;
        var roomCondition = _state.CurrentRoomCondition;

        SetPickerToValue(PatientTitlePicker, AppStateService.PatientTitleOptions, patientSummary._patientTitle);
        PatientFirstNameEntry.Text = patientSummary._patientFirstName;
        PatientMiddleNameEntry.Text = patientSummary._patientMiddleName;
        PatientLastNameEntry.Text = patientSummary._patientLastName;
        PatientSuffixEntry.Text = patientSummary._patientSuffix;
        PatientAgeEntry.Text = patientSummary._patientAge > 0 ? patientSummary._patientAge.ToString(CultureInfo.InvariantCulture) : string.Empty;
        SetPickerToValue(PatientRacePicker, AppStateService.PatientRaceOptions, patientSummary._patientRace);
        SetPickerToValue(PatientGenderPicker, AppStateService.PatientGenderAssignedAtBirthOptions, patientSummary._patientGenderAssignedAtBirth);
        SetPickerToValue(PatientReligionPicker, AppStateService.PatientReligionOptions, patientSummary._patientReligion);
        SetPickerToValue(PatientNationalityPicker, AppStateService.PatientNationalityOptions, patientSummary._patientNationality);
        PatientHeightFeetEntry.Text = patientSummary._patientHeightFeet > 0 ? patientSummary._patientHeightFeet.ToString(CultureInfo.InvariantCulture) : string.Empty;
        PatientHeightInchesEntry.Text = patientSummary._patientHeightInches > 0 ? patientSummary._patientHeightInches.ToString(CultureInfo.InvariantCulture) : string.Empty;
        PatientWeightLbsEntry.Text = patientSummary._patientWeightLbs > 0 ? patientSummary._patientWeightLbs.ToString(CultureInfo.InvariantCulture) : string.Empty;

        LightLevelEntry.Text = roomCondition._light > 0 ? roomCondition._light.ToString(CultureInfo.InvariantCulture) : string.Empty;

        var useCelsius = string.Equals(roomCondition._temperatureUnit, "Celsius", StringComparison.OrdinalIgnoreCase);
        CelsiusRadio.IsChecked = useCelsius;
        FahrenheitRadio.IsChecked = !useCelsius;
        var displayTemperature = useCelsius ? roomCondition._temperature_c : roomCondition._temperature_f;
        TemperatureValueEntry.Text = displayTemperature != 0 ? displayTemperature.ToString(CultureInfo.InvariantCulture) : string.Empty;
        BedInclineEntry.Text = roomCondition._bedInclineDegrees > 0 ? roomCondition._bedInclineDegrees.ToString(CultureInfo.InvariantCulture) : string.Empty;

        SetPickerToIntValue(CurrentPainLevelPicker, patientCondition._pain_level);
        SetPickerToIntValue(CurrentStressLevelPicker, patientCondition._stress_level);
        SetPickerToIntValue(CurrentDifficultySleepingPicker, patientCondition._sleep_quality);
    }

    private async void OnUploadNotesClicked(object sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".txt", ".docx" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select clinical notes",
                FileTypes = customFileType
            });

            if (result is null)
            {
                return;
            }

            await using var stream = await result.OpenReadAsync();
            var extension = Path.GetExtension(result.FileName).ToLowerInvariant();
            var contents = extension switch
            {
                ".txt" => await ReadTextFileAsync(stream),
                ".docx" => ReadDocxText(stream),
                _ => string.Empty
            };

            _state.CurrentPatientSummary._clinicalNotes = contents;
            UploadStatusLabel.Text = $"Loaded {result.FileName} ({contents.Length} chars).";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Upload failed", ex.Message, "OK");
        }
    }

    private static async Task<string> ReadTextFileAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static string ReadDocxText(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);
        var paragraphs = document.MainDocumentPart?.Document.Body?.Descendants<Text>()
            .Select(t => t.Text)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        return paragraphs is { Length: > 0 }
            ? string.Join(" ", paragraphs)
            : string.Empty;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var patientSummary = _state.CurrentPatientSummary;
        var patientCondition = _state.CurrentPatientCondition;
        var roomCondition = _state.CurrentRoomCondition;
        patientSummary._patientTitle = ReadPickerSelection(PatientTitlePicker);
        patientSummary._patientFirstName = (PatientFirstNameEntry.Text ?? string.Empty).Trim();
        patientSummary._patientMiddleName = (PatientMiddleNameEntry.Text ?? string.Empty).Trim();
        patientSummary._patientLastName = (PatientLastNameEntry.Text ?? string.Empty).Trim();
        patientSummary._patientSuffix = (PatientSuffixEntry.Text ?? string.Empty).Trim();
        patientSummary._patientAge = ParseIntOrDefault(PatientAgeEntry.Text);
        patientSummary._patientRace = ReadPickerSelection(PatientRacePicker);
        patientSummary._patientGenderAssignedAtBirth = ReadPickerSelection(PatientGenderPicker);
        patientSummary._patientReligion = ReadPickerSelection(PatientReligionPicker);
        patientSummary._patientNationality = ReadPickerSelection(PatientNationalityPicker);
        patientSummary._patientHeightFeet = ParseIntOrDefault(PatientHeightFeetEntry.Text);
        patientSummary._patientHeightInches = ParseIntOrDefault(PatientHeightInchesEntry.Text);
        patientSummary._patientWeightLbs = ParseDoubleOrDefault(PatientWeightLbsEntry.Text);

        _state.CurrentPatientSummary = patientSummary;

        var parsedLight = ParseDoubleOrDefault(LightLevelEntry.Text);
        roomCondition._light = Math.Clamp(parsedLight, 0d, 100d);

        var enteredTemperature = ParseDoubleOrDefault(TemperatureValueEntry.Text);
        if (CelsiusRadio.IsChecked)
        {
            roomCondition._temperatureUnit = "Celsius";
            roomCondition._temperature_c = enteredTemperature;
            roomCondition._temperature_f = (enteredTemperature * 9d / 5d) + 32d;
        }
        else
        {
            roomCondition._temperatureUnit = "Fahrenheit";
            roomCondition._temperature_f = enteredTemperature;
            roomCondition._temperature_c = (enteredTemperature - 32d) * 5d / 9d;
        }

        var parsedBedIncline = ParseDoubleOrDefault(BedInclineEntry.Text);
        roomCondition._bedInclineDegrees = Math.Clamp(parsedBedIncline, 0d, 60d);

        patientCondition._pain_level = ReadPickerIntSelection(CurrentPainLevelPicker);
        patientCondition._stress_level = ReadPickerIntSelection(CurrentStressLevelPicker);
        patientCondition._sleep_quality = ReadPickerIntSelection(CurrentDifficultySleepingPicker);

        _state.CurrentPatientCondition = patientCondition;
        _state.CurrentRoomCondition = roomCondition;
        _state.NotifyStateChanged();

        await Navigation.PopModalAsync();
    }

    private static int ParseIntOrDefault(string? text)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static double ParseDoubleOrDefault(string? text)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static string ReadPickerSelection(Picker picker)
    {
        return picker.SelectedIndex >= 0 ? picker.Items[picker.SelectedIndex] : string.Empty;
    }

    private static int ReadPickerIntSelection(Picker picker)
    {
        var selectedValue = ReadPickerSelection(picker);
        return int.TryParse(selectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, 0, 10)
            : 0;
    }

    private static void SetPickerToValue(Picker picker, IReadOnlyList<string> options, string value)
    {
        picker.SelectedIndex = Array.IndexOf(options.ToArray(), value);
    }

    private static void SetPickerToIntValue(Picker picker, double value)
    {
        var roundedValue = Math.Clamp((int)Math.Round(value, MidpointRounding.AwayFromZero), 0, 10);
        var target = roundedValue.ToString(CultureInfo.InvariantCulture);
        picker.SelectedIndex = picker.Items.IndexOf(target);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
