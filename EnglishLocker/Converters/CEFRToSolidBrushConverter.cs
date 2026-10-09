using English.Entity.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace EnglishLocker.Converters;

/// <summary>
/// Chuyển đổi chuỗi mã màu hex (ví dụ: "#6C63FF") sang SolidColorBrush để dùng trong XAML Binding.
/// </summary>
public class CEFRToSolidBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CEFRLevel level)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(GetLevelColor(level));
                return new SolidColorBrush(color);
            }
            catch
            {
                // Nếu không parse được, trả về màu mặc định
            }
        }
        else if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                // Nếu không parse được, trả về màu mặc định
            }
        }
        return new SolidColorBrush(Color.FromRgb(0x6C, 0x63, 0xFF));
    }
    private static string GetLevelColor(CEFRLevel level) => level switch
    {
        CEFRLevel.A1 => "#16A34A",
        CEFRLevel.A2 => "#65A30D",
        CEFRLevel.B1 => "#CA8A04",
        CEFRLevel.B2 => "#EA580C",
        CEFRLevel.C1 => "#DC2626",
        CEFRLevel.C2 => "#9333EA",
        _ => "#6C63FF"
    };
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
