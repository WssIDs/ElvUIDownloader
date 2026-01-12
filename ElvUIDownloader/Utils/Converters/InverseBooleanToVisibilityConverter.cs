using System.Windows;
using System.Windows.Data;

namespace ElvUIDownloader.Utils.Converters;

public class InverseBooleanToVisibilityConverter: IValueConverter
{
    #region IValueConverter Members

    public object Convert(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(Visibility))
            throw new InvalidOperationException("The target must be a Visibility");

        if (value == null) return Visibility.Collapsed;

        var bValue = (bool)value;

        var result = bValue ? Visibility.Hidden : Visibility.Collapsed;
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    #endregion
}