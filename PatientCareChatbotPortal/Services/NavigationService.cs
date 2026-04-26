using Microsoft.Extensions.DependencyInjection;

namespace PatientCareChatbotPortal.Services;

public sealed class NavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TPage>() where TPage : Page
    {
        var page = ActivatorUtilities.CreateInstance<TPage>(_serviceProvider);
        var navPage = new NavigationPage(page)
        {
            BarBackgroundColor = Color.FromArgb("#0D6EFD"),
            BarTextColor = Colors.White
        };

        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = navPage;
        }
    }

    public async Task OpenModalAsync<TPage>() where TPage : Page
    {
        var page = ActivatorUtilities.CreateInstance<TPage>(_serviceProvider);
        var modalNav = new NavigationPage(page)
        {
            BarBackgroundColor = Color.FromArgb("#0D6EFD"),
            BarTextColor = Colors.White
        };

        var rootPage = Application.Current?.Windows.Count > 0
            ? Application.Current.Windows[0].Page
            : null;

        if (rootPage is not null)
        {
            await rootPage.Navigation.PushModalAsync(modalNav);
        }
    }
}
