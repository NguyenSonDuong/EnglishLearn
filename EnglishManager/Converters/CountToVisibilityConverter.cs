using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EnglishManager.Converters;

/// <summary>
/// Chuyển đổi số nguyên (Count) sang Visibility: > 0 → Visible, ngược lại → Collapsed.
/// Sử dụng để hiển thị danh sách WordFamily khi có ít nhất 1 phần tử.
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
