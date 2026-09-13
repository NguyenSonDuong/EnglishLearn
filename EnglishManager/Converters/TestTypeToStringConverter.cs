using System.Globalization;
using System.Windows.Data;
using English.Entity.Enums;

namespace EnglishManager.Converters;

public class TestTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TestType type)
        {
            return type switch
            {
                TestType.ReverseTranslation => "Dịch ngược (Reverse Translation)",
                TestType.CollocationMatch => "Nối cặp từ (Collocation Match)",
                TestType.ContextFill => "Điền ngữ cảnh (Context Fill)",
                TestType.NaturalChoice => "Lựa chọn tự nhiên (Natural Choice)",
                TestType.MicroDictation => "Chính tả / Nghe viết (Micro Dictation)",
                _ => value.ToString() ?? string.Empty
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
