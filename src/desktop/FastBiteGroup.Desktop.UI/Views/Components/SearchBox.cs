using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace FastBiteGroup.Desktop.UI.Views.Components;

[TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]
public class SearchBox : TextBox
{
    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(SearchBox),
        new PropertyMetadata("Search..."));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(SearchBox),
        new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public ICommand ClearCommand { get; }

    public SearchBox()
    {
        ClearCommand = new RelayCommand(ClearText);
    }

    private void ClearText()
    {
        Text = string.Empty;
        Focus();
    }

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SearchBox),
            new FrameworkPropertyMetadata(typeof(SearchBox)));
    }
}
