using PatientCareChatbotPortal.Models;

namespace PatientCareChatbotPortal.Services;

public sealed class AppStateService
{
    public AppStateService()
    {
        CurrentPatientSummary = new PatientSummary
        {
            _introducedSelf = false,
            _patientTitle = "None",
            _patientFirstName = string.Empty,
            _patientMiddleName = string.Empty,
            _patientLastName = string.Empty,
            _patientSuffix = string.Empty,
            _patientAge = 0,
            _patientRace = "Prefer not to disclose",
            _patientGenderAssignedAtBirth = "Other",
            _patientReligion = "Prefer not to disclose",
            _patientNationality = "Prefer not to disclose",
            _patientHeightFeet = 0,
            _patientHeightInches = 0,
            _patientWeightLbs = 0.0,
            _clinicalNotes = string.Empty
        };

        CurrentPatientCondition = new PatientCondition
        {
            _pain_level = 0.0,
            _stress_level = 0.0,
            _sleep_quality = 0.0
        };

        CurrentRoomCondition = new RoomCondition
        {
            _light = 0.0,
            _sound = "NONE",
            _temperature_f = 70.0,
            _temperature_c = 21.1,
            _temperatureUnit = "Fahrenheit",
            _bedInclineDegrees = 0.0,
            _currentPhotoSet = string.Empty
        };
    }
    
    public static readonly string[] PatientTitleOptions =
    {
        "Mr.",
        "Ms.",
        "Mrs.",
        "None"
    };

    public static readonly string[] PatientRaceOptions =
    {
        "American Indian or Alaska Native",
        "Asian",
        "Black or African American",
        "Native Hawaiian or Other Pacific Islander",
        "White",
        "Two or more races",
        "Other",
        "Prefer not to disclose"
    };

    public static readonly string[] PatientGenderAssignedAtBirthOptions =
    {
        "Male",
        "Female",
        "Other"
    };

    public static readonly string[] PatientReligionOptions =
    {
        "Christianity",
        "Islam",
        "Hinduism",
        "Buddhism",
        "Judaism",
        "Sikhism",
        "Traditional/Indigenous",
        "No religion",
        "Other",
        "Prefer not to disclose"
    };

    public static readonly string[] PatientNationalityOptions =
    {
        "American",
        "Canadian",
        "Mexican",
        "Brazilian",
        "British",
        "French",
        "German",
        "Italian",
        "Spanish",
        "Nigerian",
        "Egyptian",
        "Indian",
        "Pakistani",
        "Chinese",
        "Japanese",
        "Korean",
        "Filipino",
        "Australian",
        "New Zealander",
        "Other",
        "Prefer not to disclose"
    };

    public static readonly string[] SoundOptions =
    {
        "ambient_guitar",
        "gentle_rain",
        "happy_park",
        "ocean",
        "calm_night",
        "rainforest",
        "soft_piano",
        "white_noise",
        "orchestra"
    };

    public static readonly string[] CurrentPhotoSetOptions =
    {
        "Family",
        "Nature",
        "Space",
        "Pets",
        "Flowers"
    };

    public struct RoomCondition
    {
        public double _light { get; set; }
        public string _sound { get; set; }
        public double _temperature_f { get; set; }
        public double _temperature_c { get; set; }
        public string _temperatureUnit { get; set; }
        public double _bedInclineDegrees { get; set; }
        public string _currentPhotoSet {get; set; }
    }

    public struct PatientSummary
    {
        public bool _introducedSelf { get; set; }
        public string _patientTitle { get; set; }
        public string _patientFirstName { get; set; }
        public string _patientMiddleName { get; set; }
        public string _patientLastName { get; set; }
        public string _patientSuffix { get; set; }
        public int _patientAge { get; set; }
        public string _patientRace { get; set; }
        public string _patientGenderAssignedAtBirth { get; set; }
        public string _patientReligion { get; set; }
        public string _patientNationality { get; set; }
        public int _patientHeightFeet { get; set; }
        public int _patientHeightInches { get; set; }
        public double _patientWeightLbs { get; set; }
        public string _clinicalNotes { get; set; }
    }

    public struct PatientCondition
    {
        public double _pain_level { get; set; }
        public double _stress_level { get; set; }
        public double _sleep_quality { get; set; }
    }

    public PatientSummary CurrentPatientSummary;
    
    public PatientCondition CurrentPatientCondition;

    public RoomCondition CurrentRoomCondition;

    public event Action? StateChanged;
    
    private bool _recordingActive = false;

    private string _currentBackgroundAudio = string.Empty;

    public string CurrentBackgroundAudio {
        get => _currentBackgroundAudio;
        set
        {
            if (_currentBackgroundAudio == value) return;
            _currentBackgroundAudio = value;
            OnStateChanged();
        }
    }

    public bool RecordingActive
    {
        get => _recordingActive;
        set
        {
            if (_recordingActive == value) return;
            _recordingActive = value;
            OnStateChanged();
        }
    }

    public void NotifyStateChanged() => OnStateChanged();

    private void OnStateChanged() => StateChanged?.Invoke();
}
