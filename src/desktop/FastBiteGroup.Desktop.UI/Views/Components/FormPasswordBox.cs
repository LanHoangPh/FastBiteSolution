using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FastBiteGroup.Desktop.UI.Views.Components;

public class FormPasswordBox : Control
{
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
        nameof(Password),
        typeof(string),
        typeof(FormPasswordBox),
        new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnPasswordPropertyChanged));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(FormPasswordBox),
        new PropertyMetadata(string.Empty));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(Geometry),
        typeof(FormPasswordBox),
        new PropertyMetadata(null));

    public Geometry Icon
    {
        get => (Geometry)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(FormPasswordBox),
        new PropertyMetadata(new CornerRadius(8)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty IsPasswordVisibleProperty = DependencyProperty.Register(
        nameof(IsPasswordVisible),
        typeof(bool),
        typeof(FormPasswordBox),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsPasswordVisibleChanged));

    public bool IsPasswordVisible
    {
        get => (bool)GetValue(IsPasswordVisibleProperty);
        set => SetValue(IsPasswordVisibleProperty, value);
    }

    private PasswordBox? _passwordBox;
    private TextBox? _textBox;
    private ToggleButton? _eyeToggle;
    private bool _isSyncing = false;

    static FormPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FormPasswordBox),
            new FrameworkPropertyMetadata(typeof(FormPasswordBox)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_passwordBox != null)
            _passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

        if (_textBox != null)
            _textBox.TextChanged -= TextBox_TextChanged;

        _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;
        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
        _eyeToggle = GetTemplateChild("PART_EyeToggle") as ToggleButton;

        if (_passwordBox != null)
        {
            _passwordBox.Password = Password;
            _passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        if (_textBox != null)
        {
            _textBox.Text = Password;
            _textBox.TextChanged += TextBox_TextChanged;
        }

        SyncPasswordToControls();
    }

    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormPasswordBox control)
        {
            control.SyncPasswordToControls();
        }
    }

    private static void OnIsPasswordVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormPasswordBox control)
        {
            control.HandlePasswordVisibilityChanged();
        }
    }

    private void HandlePasswordVisibilityChanged()
    {
        bool isFocused = (_passwordBox != null && _passwordBox.IsFocused) || (_textBox != null && _textBox.IsFocused);
        
        if (IsPasswordVisible)
        {
            if (isFocused && _textBox != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _textBox.Focus();
                    _textBox.CaretIndex = _textBox.Text.Length;
                }));
            }
        }
        else
        {
            if (isFocused && _passwordBox != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _passwordBox.Focus();
                }));
            }
        }
    }

    private void SyncPasswordToControls()
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            string newPassword = Password ?? string.Empty;
            if (_passwordBox != null && _passwordBox.Password != newPassword)
            {
                _passwordBox.Password = newPassword;
            }
            if (_textBox != null && _textBox.Text != newPassword)
            {
                _textBox.Text = newPassword;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            if (_passwordBox != null)
            {
                Password = _passwordBox.Password;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSyncing) return;
        _isSyncing = true;
        try
        {
            if (_textBox != null)
            {
                Password = _textBox.Text;
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }
}
