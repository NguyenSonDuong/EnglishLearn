using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnglishManager.Converters;

public class BoolToStatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Đang hoạt động" : "Tạm ngưng";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStatusColorConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBrush = new((Color)ColorConverter.ConvertFromString("#10B981"));
    private static readonly SolidColorBrush InactiveBrush = new((Color)ColorConverter.ConvertFromString("#9CA3AF"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? ActiveBrush : InactiveBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
