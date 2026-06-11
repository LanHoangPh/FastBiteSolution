using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastBiteGroup.Desktop.Application.Abstractions;
using FastBiteGroup.Desktop.Application.Models.Auth;
using FastBiteGroup.Desktop.UI.Services;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private string _appTitle = "FastBite Desktop";

    public string CurrentLanguageCode
    {
        get => _languageService.CurrentLanguage;
        set
        {
            if (_languageService.CurrentLanguage != value)
            {
                _languageService.SetLanguage(value);
                OnPropertyChanged(nameof(CurrentLanguageCode));
            }
        }
    }

    [ObservableProperty]
    private string _themeStatusText = string.Empty;

    public bool IsDarkTheme
    {
        get => _themeService.CurrentResolvedTheme == ResolvedTheme.Dark;
        set
        {
            if (value)
            {
                SetTheme(AppThemeMode.Dark);
            }
            else
            {
                SetTheme(AppThemeMode.Light);
            }
        }
    }

    [ObservableProperty]
    private bool _isSettingsPopupOpen;

    public bool IsSystemThemeSelected => _themeService.CurrentMode == AppThemeMode.System;
    public bool IsLightThemeSelected => _themeService.CurrentMode == AppThemeMode.Light;
    public bool IsDarkThemeSelected => _themeService.CurrentMode == AppThemeMode.Dark;

    public ObservableCollection<DashboardMetricViewModel> DashboardMetrics { get; } = new();
    public ObservableCollection<AccessLogEntryViewModel> AccessLogs { get; } = new();
    
    public ObservableCollection<SidebarItemViewModel> SidebarItems { get; } = new();
    public ObservableCollection<ConversationItemViewModel> Conversations { get; } = new();

    [ObservableProperty]
    private SidebarItemViewModel? _selectedSidebarItem;

    partial void OnSelectedSidebarItemChanged(SidebarItemViewModel? oldValue, SidebarItemViewModel? newValue)
    {
        if (oldValue != null) oldValue.IsSelected = false;
        if (newValue != null) newValue.IsSelected = true;
    }

    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;

    public UserDto? CurrentUser => _currentUserService.User;

    public string UserInitials
    {
        get
        {
            if (CurrentUser == null) return "UA";
            
            var firstName = CurrentUser.FirstName?.Trim() ?? string.Empty;
            var lastName = CurrentUser.LastName?.Trim() ?? string.Empty;
            
            if (firstName.Length > 0 && lastName.Length > 0)
            {
                return $"{lastName[0]}{firstName[0]}".ToUpper();
            }
            if (firstName.Length > 0)
            {
                return firstName[..Math.Min(2, firstName.Length)].ToUpper();
            }
            if (lastName.Length > 0)
            {
                return lastName[..Math.Min(2, lastName.Length)].ToUpper();
            }
            if (!string.IsNullOrEmpty(CurrentUser.Email))
            {
                return CurrentUser.Email[..Math.Min(2, CurrentUser.Email.Length)].ToUpper();
            }
            return "UA";
        }
    }

    public event System.Action? LogoutSuccessful;

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        LogoutSuccessful?.Invoke();
    }

    public MainWindowViewModel(
        IThemeService themeService, 
        ILanguageService languageService,
        ICurrentUserService currentUserService,
        IAuthService authService)
    {
        _themeService = themeService;
        _languageService = languageService;
        _currentUserService = currentUserService;
        _authService = authService;
        UpdateThemeStatus();

        // Conversations mock data matching the Zalo screenshot
        Conversations.Add(new ConversationItemViewModel("LM", "Đánh boss LM", "đáp án là gì", "4 giờ", 0, false));
        Conversations.Add(new ConversationItemViewModel("NH", "Ngân hàng Nhà Nước 🏦", "Hoàng Sơn: @Ngọc Khánh khoá a...", "4 giờ", 30, true));
        Conversations.Add(new ConversationItemViewModel("MK", "Ma Kiếm ⚔️", "BXH THẦN BINH KHAI MỞ...", "5 giờ", 0, true));
        Conversations.Add(new ConversationItemViewModel("ME", "Mẹ ❤️", "📹 Cuộc gọi video đến", "22 giờ", 0, false));
        Conversations.Add(new ConversationItemViewModel("NT", "Nguyen Thanh", "E sẽ nhận mail vào ngày mai nhé", "Hôm qua", 0, true));
        Conversations.Add(new ConversationItemViewModel("DY", "Đào Hải Yến", "Chia ra như vậy sẽ đỡ hơn chút", "Hôm qua", 0, false));
        Conversations.Add(new ConversationItemViewModel("PL", "Phương Lan", "C cảm ơn nhé", "Hôm qua", 0, true));
        Conversations.Add(new ConversationItemViewModel("MD", "My Documents", "Bạn: 📄 [Hình ảnh]", "2 ngày", 0, false));
        Conversations.Add(new ConversationItemViewModel("CH", "Chị 😃", "📞 Cuộc gọi thoại đến", "3 ngày", 0, true));
        Conversations.Add(new ConversationItemViewModel("LI", "Liobank by VPBank 💳", "KỂ CHUYỆN CÙNG LIO RINH...", "6 ngày", 1, false));
        Conversations.Add(new ConversationItemViewModel("QH", "Quân Hiệp Truyện 🛡️", "BẢNG VÀNG LỰC CHIẾN...", "6 ngày", 0, true));

        // Sample data
        DashboardMetrics.Add(new DashboardMetricViewModel("Active Sessions", "1,204", "+15% from last week"));
        DashboardMetrics.Add(new DashboardMetricViewModel("API Requests", "84K", "Avg 120ms latency"));
        DashboardMetrics.Add(new DashboardMetricViewModel("CPU Load", "12%", "Normal operating range"));

        AccessLogs.Add(new AccessLogEntryViewModel("17:35:23", "Alex Mercer", "192.168.1.50", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:38:10", "Jane Doe", "10.0.0.12", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:40:02", "John Smith", "172.16.2.8", "Failed"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:42:15", "Alice Cooper", "192.168.1.102", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:45:50", "Bob Marley", "192.168.1.15", "Failed"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:48:11", "Charlie Brown", "10.0.0.5", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:51:30", "David Miller", "172.16.5.21", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:55:00", "Emma Watson", "192.168.2.11", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("17:58:45", "Frank Castle", "10.0.0.100", "Success"));
        AccessLogs.Add(new AccessLogEntryViewModel("18:02:10", "Grace Hopper", "192.168.1.1", "Success"));

        SidebarItems.Add(new SidebarItemViewModel("Dashboard", "M13,3V9H21V3M13,21H21V11H13M3,21H11V15H3M3,13H11V3H3V13Z"));
        SidebarItems.Add(new SidebarItemViewModel("Conversations", "M20,2H4A2,2 0 0,0 2,4V22L6,18H20A2,2 0 0,0 22,16V4A2,2 0 0,0 20,2Z"));
        SidebarItems.Add(new SidebarItemViewModel("Settings", "M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z"));
        SelectedSidebarItem = SidebarItems[0];
    }

    [RelayCommand]
    private void SetSystemTheme() => SetTheme(AppThemeMode.System);

    [RelayCommand]
    private void SetLightTheme() => SetTheme(AppThemeMode.Light);

    [RelayCommand]
    private void SetDarkTheme() => SetTheme(AppThemeMode.Dark);

    [RelayCommand]
    private void ToggleSettingsPopup() => IsSettingsPopupOpen = !IsSettingsPopupOpen;

    private void SetTheme(AppThemeMode mode)
    {
        _themeService.SetTheme(mode);
        
        // Notify UI to apply Syncfusion theme in code behind, or do it via an event
        OnThemeChanged();

        UpdateThemeStatus();
        IsSettingsPopupOpen = false;
    }

    // Event to let view know when theme changed (to call ApplySyncfusionTheme)
    public event System.Action? ThemeChanged;

    private void OnThemeChanged()
    {
        ThemeChanged?.Invoke();
    }

    private void UpdateThemeStatus()
    {
        ThemeStatusText = _themeService.CurrentMode == AppThemeMode.System
            ? $"Theme: System ({_themeService.CurrentResolvedTheme})"
            : $"Theme: {_themeService.CurrentMode}";

        OnPropertyChanged(nameof(IsSystemThemeSelected));
        OnPropertyChanged(nameof(IsLightThemeSelected));
        OnPropertyChanged(nameof(IsDarkThemeSelected));
        OnPropertyChanged(nameof(IsDarkTheme));
    }
}
