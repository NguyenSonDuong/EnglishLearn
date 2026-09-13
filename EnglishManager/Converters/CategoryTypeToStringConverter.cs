using System.Globalization;
using System.Windows.Data;
using English.Entity.Enums;

namespace EnglishManager.Converters;

public class CategoryTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CategoryType type)
        {
            return type switch
            {
                CategoryType.Vocab => "Từ vựng (Vocab)",
                CategoryType.PhrasalVerb => "Cụm động từ (Phrasal Verb)",
                CategoryType.Collocation => "Cụm từ kết hợp (Collocation)",
                CategoryType.Slang => "Tiếng lóng (Slang)",
                CategoryType.Sentence => "Mẫu câu (Sentence)",
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
