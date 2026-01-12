using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace ElvUIDownloader.Utils.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Enum e ? GetDescription(e) : value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private string? GetDescription(Enum value)
    {
        var fi = value.GetType()
            .GetField(value.ToString());

        if (fi == null) return null;

        var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().ToList();

        return attributes.Count > 0 ? attributes.FirstOrDefault()?.Description : value.ToString();
    }
}