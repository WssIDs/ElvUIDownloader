using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ElvUIDownloader.Utils.Converters;

public class TimeSpanToBooleanConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
            return ts.TotalSeconds <= 5;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}