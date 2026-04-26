using Microsoft.Extensions.DependencyInjection;
using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Pages;

public partial class StartSessionPage : ContentPage
{
    private readonly NavigationService _navigationService;

    public StartSessionPage()
    {
        InitializeComponent();
        _navigationService = Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<NavigationService>()
            ?? throw new InvalidOperationException("Navigation service unavailable.");
    }

    private void OnStartSessionClicked(object sender, EventArgs e)
    {
        SessionStatusLabel.Text = $"Session started at {DateTime.Now:t}.";
        _navigationService.NavigateTo<UserPage>();
    }
}
