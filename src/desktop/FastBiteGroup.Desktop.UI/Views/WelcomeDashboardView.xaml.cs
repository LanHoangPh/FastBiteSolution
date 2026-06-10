using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FastBiteGroup.Desktop.UI.Views;

public partial class WelcomeDashboardView : UserControl
{
    private readonly DispatcherTimer _timer;

    public WelcomeDashboardView()
    {
        InitializeComponent();

        // 1. Setup DispatcherTimer for auto-rotation (1.5 seconds)
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        // 2. Safely highlight initial slide's dot when control is fully loaded in visual tree
        Loaded += (s, e) => UpdateDots(CarouselTabControl.SelectedIndex);

        // 3. Stop timer when control is unloaded to prevent background threads/memory leaks
        Unloaded += (s, e) => _timer.Stop();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        int totalItems = CarouselTabControl.Items.Count;
        if (totalItems > 0)
        {
            // Auto-advance to next slide with wrapping
            CarouselTabControl.SelectedIndex = (CarouselTabControl.SelectedIndex + 1) % totalItems;
        }
    }

    private void ResetTimer()
    {
        _timer.Stop();
        _timer.Start();
    }

    private void PrevBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        int totalItems = CarouselTabControl.Items.Count;
        if (totalItems > 0)
        {
            CarouselTabControl.SelectedIndex = (CarouselTabControl.SelectedIndex - 1 + totalItems) % totalItems;
            ResetTimer(); // Reset timer so auto-advance doesn't trigger immediately after manual click
        }
    }

    private void NextBtn_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        int totalItems = CarouselTabControl.Items.Count;
        if (totalItems > 0)
        {
            CarouselTabControl.SelectedIndex = (CarouselTabControl.SelectedIndex + 1) % totalItems;
            ResetTimer(); // Reset timer so auto-advance doesn't trigger immediately after manual click
        }
    }

    private void CarouselTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Dot0 == null || Dot1 == null || Dot2 == null) return;
        UpdateDots(CarouselTabControl.SelectedIndex);
    }

    private void UpdateDots(int activeIndex)
    {
        if (Dot0 == null || Dot1 == null || Dot2 == null) return;

        var activeBrush = (System.Windows.Media.Brush)FindResource("PrimaryBrush");
        var inactiveBrush = (System.Windows.Media.Brush)FindResource("BorderBrush");
        
        Dot0.Fill = activeIndex == 0 ? activeBrush : inactiveBrush;
        Dot1.Fill = activeIndex == 1 ? activeBrush : inactiveBrush;
        Dot2.Fill = activeIndex == 2 ? activeBrush : inactiveBrush;
    }
}
