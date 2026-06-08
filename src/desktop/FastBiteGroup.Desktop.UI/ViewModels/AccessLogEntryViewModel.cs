namespace FastBiteGroup.Desktop.UI.ViewModels;

public sealed record AccessLogEntryViewModel(
    string Time,
    string User,
    string IpAddress,
    string Status);
