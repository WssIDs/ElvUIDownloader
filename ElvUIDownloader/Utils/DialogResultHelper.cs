using System.Windows;

namespace ElvUIDownloader.Utils;

public static class DialogResultHelper
{
    public static readonly DependencyProperty DialogResultProperty =
        DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool?),
            typeof(DialogResultHelper),
            new PropertyMetadata(null, OnDialogResultChanged));

    public static void SetDialogResult(Window target, bool? value)
    {
        target.SetValue(DialogResultProperty, value);
    }

    public static bool? GetDialogResult(Window target)
    {
        return (bool?)target.GetValue(DialogResultProperty);
    }

    private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Window window)
        {
            // Устанавливаем DialogResult только если окно открыто модально
            if (window.IsLoaded && window.WindowStartupLocation != WindowStartupLocation.Manual)
            {
                window.DialogResult = (bool?)e.NewValue;
            }
        }
    }
}