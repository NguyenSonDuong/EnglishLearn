using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EnglishLocker.Converters;

/// <summary>
/// Chuyển đổi chuỗi: Visible nếu chuỗi không rỗng/null, Collapsed nếu rỗng.
/// Dùng để ẩn/hiện panel thông báo lỗi ErrorMessage.
/// </summary>
public class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
