using System.Globalization;
using System.Windows.Data;

namespace EnglishLocker.Converters;

/// <summary>
/// Đảo ngược giá trị Boolean: True → False, False → True.
/// Dùng để disable các button khi IsLoading = true.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
