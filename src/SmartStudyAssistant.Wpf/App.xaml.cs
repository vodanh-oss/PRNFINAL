using System.IO;
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
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            MessageBox.Show($"Lỗi hệ thống: {args.ExceptionObject}", "Lỗi không xác định");
        };

        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Initialize SQLite Database
            var db = ServiceProvider.GetRequiredService<IDatabaseService>();
            await db.InitializeAsync();

            var mainVm = ServiceProvider.GetRequiredService<MainViewModel>();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = mainVm;
            MainWindow = mainWindow;
            mainWindow.Show();

            await mainVm.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Lỗi khởi động Smart Study Assistant");
            Shutdown(-1);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        var secureStorage = new DpapiSecureStorage();
        services.AddSingleton<ISecureStorage>(secureStorage);

        string connectionString = LoadConnectionString(secureStorage);
        services.AddSingleton<IDatabaseService>(new SqlServerDatabaseService(connectionString));
        services.AddSingleton<IRagEngine, VectorRagEngine>();
        services.AddSingleton<AiServiceCoordinator>();

        // ViewModels - 4 chuyên mục rõ ràng
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddSingleton<DocumentsViewModel>();
        services.AddSingleton<FlashcardsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    private static string LoadConnectionString(ISecureStorage secureStorage)
    {
        // 1. Kiểm tra chuỗi kết nối người dùng đã lưu tùy chỉnh qua giao diện Cấu hình
        var savedCustom = secureStorage.GetSecret("SqlConnectionString");
        if (!string.IsNullOrWhiteSpace(savedCustom))
        {
            return savedCustom.Trim();
        }

        // 2. Đọc từ tệp cấu hình appsettings.json nếu có
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                    cs.TryGetProperty("DefaultConnection", out var val))
                {
                    var fileConn = val.GetString();
                    if (!string.IsNullOrWhiteSpace(fileConn))
                    {
                        return fileConn.Trim();
                    }
                }
            }
        }
        catch
        {
            // Dự phòng nếu đọc tệp lỗi
        }

        // 3. Dự phòng mặc định localhost
        return SqlServerDatabaseService.DefaultConnectionString;
    }
}
