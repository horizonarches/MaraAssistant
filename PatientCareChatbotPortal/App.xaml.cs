namespace PatientCareChatbotPortal;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = new NavigationPage(new Pages.StartSessionPage())
        {
            BarBackgroundColor = Color.FromArgb("#0D6EFD"),
            BarTextColor = Colors.White
        };
        return new Window(navPage)
        {
            Title = "Mara - A chatbot to help your room help you..."
        };
    }
}
