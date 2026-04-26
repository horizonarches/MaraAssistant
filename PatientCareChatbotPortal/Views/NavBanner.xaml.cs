using Microsoft.Extensions.DependencyInjection;
using PatientCareChatbotPortal.Pages;
using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Views;

public partial class NavBanner : ContentView
{
    public NavBanner()
    {
        InitializeComponent();
    }

    private NavigationService NavigationService =>
        Application.Current?.Handler?.MauiContext?.Services.GetRequiredService<NavigationService>()
        ?? throw new InvalidOperationException("Navigation service is not available.");

    private void OnStartSessionClicked(object? sender, EventArgs e) => NavigationService.NavigateTo<StartSessionPage>();
    private void OnAdminClicked(object? sender, EventArgs e) => NavigationService.NavigateTo<AdminPage>();
    private void OnUserClicked(object? sender, EventArgs e) => NavigationService.NavigateTo<UserPage>();
    private void OnInfoClicked(object? sender, EventArgs e) => NavigationService.NavigateTo<InfoPage>();
}
