using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace EnglishLocker.Converters;

/// <summary>
/// Chuyển đổi chuỗi mã màu hex (ví dụ: "#6C63FF") sang SolidColorBrush để dùng trong XAML Binding.
/// </summary>
public class StringToSolidBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hexColor && !string.IsNullOrWhiteSpace(hexColor))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                return new SolidColorBrush(color);
            }
            catch
            {
                // Nếu không parse được, trả về màu mặc định
            }
        }
        return new SolidColorBrush(Color.FromRgb(0x6C, 0x63, 0xFF));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
