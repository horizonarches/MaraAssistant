using PatientCareChatbotPortal.Services;

namespace PatientCareChatbotPortal.Pages;

public partial class InfoPage : ContentPage
{
    private readonly BuildInfoService _buildInfoService;

    public InfoPage(BuildInfoService buildInfoService)
    {
        InitializeComponent();
        _buildInfoService = buildInfoService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var info = await _buildInfoService.GetBuildInfoAsync();
        BuildSummaryEditor.Text = info.BuildSummary;
        AuthorEditor.Text = info.AuthorCopyright;
        DescriptionEditor.Text = info.AppDescription;
    }
}
