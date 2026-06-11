using System.Windows;
using System.Windows.Controls;

namespace FastBiteGroup.Desktop.UI.Views;

public partial class NavigationRailView : UserControl
{
    public NavigationRailView()
    {
        InitializeComponent();
    }

    private void Avatar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.ContextMenu != null)
        {
            element.ContextMenu.PlacementTarget = element;
            element.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }
}
