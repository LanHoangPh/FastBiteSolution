using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FastBiteGroup.Desktop.UI.ViewModels;

public partial class SidebarItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private Geometry? _iconGeometry;

    [ObservableProperty]
    private bool _isSelected;

    public SidebarItemViewModel(string title, string iconPathData)
    {
        Title = title;
        IconGeometry = string.IsNullOrEmpty(iconPathData) ? null : Geometry.Parse(iconPathData);
    }
}
