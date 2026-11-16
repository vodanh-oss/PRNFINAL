using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public partial class ProviderItemViewModel : ObservableObject
{
    public AiProviderType ProviderType { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _modelName = string.Empty;

    [ObservableProperty]
    private string _baseUrl = string.Empty;

    [ObservableProperty]
    private bool _isDefault;

    [ObservableProperty]
    private string _testStatus = "Chưa kiểm tra";

    [ObservableProperty]
    private bool? _isHealthy;
}

public partial class SettingsViewModel : ObservableObject
{
    private readonly AiServiceCoordinator _aiCoordinator;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<ProviderItemViewModel> _providers = new();

    [ObservableProperty]
    private ProviderItemViewModel? _selectedProvider;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _globalStatusMessage = string.Empty;

    [ObservableProperty]
    private string _sqlConnectionString = SqlServerDatabaseService.DefaultConnectionString;

    [ObservableProperty]
    private string _sqlConnectionStatus = "Chưa kiểm tra";

    [ObservableProperty]
    private bool? _isSqlConnected;

    public SettingsViewModel(AiServiceCoordinator aiCoordinator, IDatabaseService databaseService)
    {
        _aiCoordinator = aiCoordinator;
        _databaseService = databaseService;
        SqlConnectionString = _databaseService.ConnectionString;

        LoadProviders();
    }

    private void LoadProviders()
    {
        Providers.Clear();
        foreach (var p in _aiCoordinator.GetAllProviders())
        {
            var item = new ProviderItemViewModel
            {
                ProviderType = p.ProviderType,
                DisplayName = p.Config.DisplayName,
                ApiKey = p.Config.ApiKey,
                ModelName = p.Config.ModelName,
                BaseUrl = p.Config.BaseUrl,
                IsDefault = p.ProviderType == _aiCoordinator.CurrentProviderType
            };
            Providers.Add(item);
        }

        SelectedProvider = Providers.FirstOrDefault(p => p.IsDefault) ?? Providers.FirstOrDefault();
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (SelectedProvider == null) return;

        IsBusy = true;
        SelectedProvider.TestStatus = "Đang kiểm tra kết nối...";
        SelectedProvider.IsHealthy = null;

        try
        {
            var tempConfig = new AiProviderConfig
            {
                ProviderType = SelectedProvider.ProviderType,
                DisplayName = SelectedProvider.DisplayName,
                ApiKey = SelectedProvider.ApiKey,
                ModelName = SelectedProvider.ModelName,
                BaseUrl = SelectedProvider.BaseUrl
            };
            _aiCoordinator.UpdateProviderConfig(tempConfig);

            var provider = _aiCoordinator.GetProvider(SelectedProvider.ProviderType);
            var (success, msg) = await provider.TestConnectionAsync();

            SelectedProvider.IsHealthy = success;
            SelectedProvider.TestStatus = success ? "✓ " + msg : "✗ " + msg;
            GlobalStatusMessage = SelectedProvider.TestStatus;
        }
        catch (Exception ex)
        {
            SelectedProvider.IsHealthy = false;
            SelectedProvider.TestStatus = "✗ Lỗi: " + ex.Message;
            GlobalStatusMessage = SelectedProvider.TestStatus;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (SelectedProvider == null) return;

        foreach (var p in Providers)
        {
            var config = new AiProviderConfig
            {
                ProviderType = p.ProviderType,
                DisplayName = p.DisplayName,
                ApiKey = p.ApiKey,
                ModelName = p.ModelName,
                BaseUrl = p.BaseUrl,
                IsDefault = p.IsDefault
            };
            _aiCoordinator.UpdateProviderConfig(config);
        }

        GlobalStatusMessage = "✓ Đã lưu cấu hình AI thành công (Bảo mật qua Windows DPAPI)!";
    }

    [RelayCommand]
    private void SetAsDefault(ProviderItemViewModel item)
    {
        foreach (var p in Providers)
        {
            p.IsDefault = (p == item);
        }
        _aiCoordinator.CurrentProviderType = item.ProviderType;
        SaveSettings();
    }

    [RelayCommand]
    private async Task TestSqlConnectionAsync()
    {
        IsBusy = true;
        SqlConnectionStatus = "Đang kết nối tới SQL Server (SSMS)...";
        IsSqlConnected = null;

        try
        {
            _databaseService.ConnectionString = SqlConnectionString.Trim();
            bool connected = await _databaseService.TestConnectionAsync();
            IsSqlConnected = connected;
            SqlConnectionStatus = connected 
                ? "✓ Kết nối thành công tới SQL Server (SSMS)! Database: SmartStudyDb" 
                : "✗ Không thể kết nối tới SQL Server với chuỗi kết nối trên.";
            GlobalStatusMessage = SqlConnectionStatus;
        }
        catch (Exception ex)
        {
            IsSqlConnected = false;
            SqlConnectionStatus = "✗ Lỗi kết nối SQL Server: " + ex.Message;
            GlobalStatusMessage = SqlConnectionStatus;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetSampleDataAsync()
    {
        IsBusy = true;
        try
        {
            await _databaseService.SeedSampleDataAsync(force: true);
            GlobalStatusMessage = "✓ Đã khởi tạo lại toàn bộ dữ liệu mẫu trong SQL Server (SSMS) thành công!";
        }
        catch (Exception ex)
        {
            GlobalStatusMessage = "✗ Lỗi khi nạp dữ liệu mẫu: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
