using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VotingPCNew.Controls;

public partial class RadioTextBox : UserControl
{
    private static readonly DoubleAnimation s_fadeInAnimation = new(0, 1, new(TimeSpan.FromMilliseconds(250)))
    {
        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
    };
    private static readonly DoubleAnimation s_fadeOutAnimation = new(1, 0, new(TimeSpan.FromMilliseconds(200)))
    {
        EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
    };

    public RadioTextBox()
    {
        InitializeComponent();
    }

    // Should be a Dependency property, but this works, and doesn't need to be over-engineered.
    // We don't need the advanced stuff for now
    public bool IsChecked
    {
        get => (bool)checkBox.IsChecked;
        set {
            if (checkBox.IsChecked == value) return;
            checkBox.IsChecked = value;
            OnIsCheckedChanged(value);
        }
    }
    
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    private void OnIsCheckedChanged(bool isChecked)
    {
        // fade out if checked, fade in otherwise
        var animation = isChecked ? s_fadeOutAnimation : s_fadeInAnimation;
        crossLine.BeginAnimation(OpacityProperty, animation);
    }

    private void checkBox_Checked(object sender, RoutedEventArgs e)
    {
        OnIsCheckedChanged(true);
    }

    private void checkBox_Unchecked(object sender, RoutedEventArgs e)
    {
        OnIsCheckedChanged(false);
    }

    // XAML backing field for the shown text
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(RadioTextBox));
}