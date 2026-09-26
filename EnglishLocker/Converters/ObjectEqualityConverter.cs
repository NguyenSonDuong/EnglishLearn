using System.Globalization;
using System.Windows.Data;

namespace EnglishLocker.Converters;

/// <summary>
/// Converter kiểm tra tham chiếu bằng nhau giữa 2 object (MultiBinding).
/// Dùng để highlight ô cấp độ được chọn trong SelectLevelControl.
/// </summary>
public class ObjectEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length != 2) return false;
        return Equals(values[0], values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
