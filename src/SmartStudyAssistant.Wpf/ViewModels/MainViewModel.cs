using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartStudyAssistant.Core.Models;
using SmartStudyAssistant.Core.Services.Ai;
using SmartStudyAssistant.Core.Services.Database;

namespace SmartStudyAssistant.Wpf.ViewModels;

public enum NavigationTab
{
    Chat,
    Documents,
    Flashcards,
    Settings
}

public partial class MainViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;

    public ChatViewModel ChatVm { get; }
    public DocumentsViewModel DocumentsVm { get; }
    public FlashcardsViewModel FlashcardsVm { get; }
    public SettingsViewModel SettingsVm { get; }

    [ObservableProperty]
    private NavigationTab _selectedTab = NavigationTab.Chat;

    [ObservableProperty]
    private object _currentView = null!;

    [ObservableProperty]
    private string _activeTab = "Chat";

    [ObservableProperty]
    private AiProviderType _activeProviderType;

    [ObservableProperty]
    private string _appTitle = "Smart Study Assistant - Trợ lý Ôn thi & Học tập Sinh viên";

    public MainViewModel(
        IDatabaseService databaseService,
        AiServiceCoordinator aiCoordinator,
        ChatViewModel chatVm,
        DocumentsViewModel documentsVm,
        FlashcardsViewModel flashcardsVm,
        SettingsViewModel settingsVm)
    {
        _databaseService = databaseService;
        _aiCoordinator = aiCoordinator;

        ChatVm = chatVm;
        DocumentsVm = documentsVm;
        FlashcardsVm = flashcardsVm;
        SettingsVm = settingsVm;

        CurrentView = ChatVm;
        ActiveTab = "Chat";

        ActiveProviderType = _aiCoordinator.CurrentProviderType;

        _aiCoordinator.ProviderChanged += (type) =>
        {
            ActiveProviderType = type;
        };
    }

    public async Task InitializeAsync()
    {
        await _databaseService.InitializeAsync();
        await ChatVm.InitializeAsync();
        await DocumentsVm.InitializeAsync();
        await FlashcardsVm.InitializeAsync();
    }

    [RelayCommand]
    private void Navigate(string tab)
    {
        ActiveTab = tab;
        CurrentView = tab switch
        {
            "Chat" => ChatVm,
            "Documents" => DocumentsVm,
            "Flashcards" => FlashcardsVm,
            "Settings" => SettingsVm,
            _ => ChatVm
        };

        SelectedTab = tab switch
        {
            "Chat" => NavigationTab.Chat,
            "Documents" => NavigationTab.Documents,
            "Flashcards" => NavigationTab.Flashcards,
            "Settings" => NavigationTab.Settings,
            _ => NavigationTab.Chat
        };
    }

    [RelayCommand]
    private void SwitchProvider(AiProviderType type)
    {
        _aiCoordinator.CurrentProviderType = type;
        ActiveProviderType = type;
    }
}
