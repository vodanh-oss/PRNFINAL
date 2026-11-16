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
    Pomodoro,
    Progress,
    Settings
}

public partial class MainViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;
    private readonly AiServiceCoordinator _aiCoordinator;

    public ChatViewModel ChatVm { get; }
    public DocumentsViewModel DocumentsVm { get; }
    public FlashcardsViewModel FlashcardsVm { get; }
    public PomodoroViewModel PomodoroVm { get; }
    public ProgressViewModel ProgressVm { get; }
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
    private bool _isDarkTheme = true;

    [ObservableProperty]
    private string _appTitle = "Smart Study Assistant - Trợ lý Ôn thi & Học tập Sinh viên";

    public MainViewModel(
        IDatabaseService databaseService,
        AiServiceCoordinator aiCoordinator,
        ChatViewModel chatVm,
        DocumentsViewModel documentsVm,
        FlashcardsViewModel flashcardsVm,
        PomodoroViewModel pomodoroVm,
        ProgressViewModel progressVm,
        SettingsViewModel settingsVm)
    {
        _databaseService = databaseService;
        _aiCoordinator = aiCoordinator;

        ChatVm = chatVm;
        DocumentsVm = documentsVm;
        FlashcardsVm = flashcardsVm;
        PomodoroVm = pomodoroVm;
        ProgressVm = progressVm;
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
        await PomodoroVm.InitializeAsync();
        await ProgressVm.InitializeAsync();
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
            "Pomodoro" => PomodoroVm,
            "Progress" => ProgressVm,
            "Settings" => SettingsVm,
            _ => ChatVm
        };

        if (Enum.TryParse<NavigationTab>(tab, out var parsedTab))
        {
            SelectedTab = parsedTab;
        }
    }

    public void NavigateToTab(NavigationTab tab)
    {
        Navigate(tab.ToString());
    }

    [RelayCommand]
    private void SwitchProvider(AiProviderType type)
    {
        _aiCoordinator.CurrentProviderType = type;
        ActiveProviderType = type;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }
}
