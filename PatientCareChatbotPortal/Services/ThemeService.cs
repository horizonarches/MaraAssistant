namespace PatientCareChatbotPortal.Services;

public sealed class ThemeService
{
    public void ApplyTheme(string? themeTag)
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        switch ((themeTag ?? "default").Trim().ToLowerInvariant())
        {
            case "alert":
                resources["AppBackground"] = Color.FromArgb("#FFF3F3");
                resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                resources["AppText"] = Color.FromArgb("#4A0A0A");
                resources["BootstrapPrimary"] = Color.FromArgb("#DC3545");
                break;
            case "calm":
                resources["AppBackground"] = Color.FromArgb("#F0FBFF");
                resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                resources["AppText"] = Color.FromArgb("#11303A");
                resources["BootstrapPrimary"] = Color.FromArgb("#0DCAF0");
                break;
            case "night":
                resources["AppBackground"] = Color.FromArgb("#1E1E2F");
                resources["CardBackground"] = Color.FromArgb("#2A2A40");
                resources["AppText"] = Color.FromArgb("#E9ECEF");
                resources["BootstrapPrimary"] = Color.FromArgb("#6F42C1");
                break;
            default:
                resources["AppBackground"] = Color.FromArgb("#F8F9FA");
                resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                resources["AppText"] = Color.FromArgb("#212529");
                resources["BootstrapPrimary"] = Color.FromArgb("#0D6EFD");
                break;
        }
    }
}
