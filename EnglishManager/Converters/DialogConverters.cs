using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EnglishManager.Services;

namespace EnglishManager.Converters;

public class DialogTypeToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush InfoBrush = new((Color)ColorConverter.ConvertFromString("#3B82F6"));
    private static readonly SolidColorBrush SuccessBrush = new((Color)ColorConverter.ConvertFromString("#10B981"));
    private static readonly SolidColorBrush WarningBrush = new((Color)ColorConverter.ConvertFromString("#F59E0B"));
    private static readonly SolidColorBrush ErrorBrush = new((Color)ColorConverter.ConvertFromString("#EF4444"));
    private static readonly SolidColorBrush ConfirmBrush = new((Color)ColorConverter.ConvertFromString("#6366F1"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DialogType type)
        {
            return type switch
            {
                DialogType.Info => InfoBrush,
                DialogType.Success => SuccessBrush,
                DialogType.Warning => WarningBrush,
                DialogType.Error => ErrorBrush,
                DialogType.Confirmation => ConfirmBrush,
                _ => InfoBrush
            };
        }
        return InfoBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class DialogTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DialogType type)
        {
            return type switch
            {
                DialogType.Info => "ℹ",
                DialogType.Success => "✓",
                DialogType.Warning => "⚠",
                DialogType.Error => "✕",
                DialogType.Confirmation => "?",
                _ => "ℹ"
            };
        }
        return "ℹ";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
