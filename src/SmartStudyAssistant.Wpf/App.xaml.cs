using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;
using SmartStudyAssistant.Core.Services.Rag;
using SmartStudyAssistant.Core.Services.Security;
using SmartStudyAssistant.Wpf.ViewModels;

namespace SmartStudyAssistant.Wpf;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        var mainVm = ServiceProvider.GetRequiredService<MainViewModel>();
        await mainVm.InitializeAsync();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = mainVm;
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<ISecureStorage, DpapiSecureStorage>();
        services.AddSingleton<IDatabaseService, SqliteDatabaseService>();
        services.AddSingleton<IRagEngine, VectorRagEngine>();
        services.AddSingleton<AiServiceCoordinator>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddSingleton<DocumentsViewModel>();
        services.AddSingleton<FlashcardsViewModel>();
        services.AddSingleton<PomodoroViewModel>();
        services.AddSingleton<ProgressViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }
}
