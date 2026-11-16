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
    private string _databasePath = string.Empty;

    public SettingsViewModel(AiServiceCoordinator aiCoordinator, IDatabaseService databaseService)
    {
        _aiCoordinator = aiCoordinator;
        _databaseService = databaseService;
        DatabasePath = _databaseService.DatabasePath;
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
        SelectedProvider.TestStatus = "Đang kiểm tra...";
        SelectedProvider.IsHealthy = null;

        try
        {
            // Save temporary config to coordinator to test
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

        GlobalStatusMessage = "Đã lưu cài đặt và mã hóa bảo mật an toàn qua DPAPI!";
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
    private void OpenDatabaseFolder()
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            GlobalStatusMessage = "Không thể mở thư mục: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task ResetSampleDataAsync()
    {
        IsBusy = true;
        try
        {
            await _databaseService.SeedSampleDataAsync(force: true);
            GlobalStatusMessage = "✓ Đã khởi tạo lại dữ liệu bài giảng, flashcards và checklist môn học mẫu thành công!";
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
