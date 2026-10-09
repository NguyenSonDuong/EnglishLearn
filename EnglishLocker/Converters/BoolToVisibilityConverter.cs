using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EnglishLocker.Converters;

/// <summary>
/// Chuyển đổi giá trị boolean sang Visibility (True -> Visible, False -> Collapsed).
/// Hỗ trợ đảo ngược (Inverted) nếu tham số ConverterParameter là "Inverse".
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool boolVal = value switch
        {
            bool b => b,
            null => false,
            _ => true
        };

        if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
        {
            boolVal = !boolVal;
        }

        return boolVal ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;
            if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }
            return result;
        }

        return false;
    }
}
