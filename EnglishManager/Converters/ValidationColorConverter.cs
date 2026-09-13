using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnglishManager.Converters;

public class ValidationColorConverter : IValueConverter
{
    private static readonly SolidColorBrush ValidBrush = new((Color)ColorConverter.ConvertFromString("#10B981"));
    private static readonly SolidColorBrush InvalidBrush = new((Color)ColorConverter.ConvertFromString("#EF4444"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? ValidBrush : InvalidBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
