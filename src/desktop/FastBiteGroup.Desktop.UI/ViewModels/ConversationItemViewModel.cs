using CommunityToolkit.Mvvm.ComponentModel;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class ConversationItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _initials = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _lastMessage = string.Empty;

    [ObservableProperty]
    private string _timeText = string.Empty;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private bool _isOnline;

    public ConversationItemViewModel(string initials, string name, string lastMessage, string timeText, int unreadCount, bool isOnline)
    {
        Initials = initials;
        Name = name;
        LastMessage = lastMessage;
        TimeText = timeText;
        UnreadCount = unreadCount;
        IsOnline = isOnline;
    }
}
