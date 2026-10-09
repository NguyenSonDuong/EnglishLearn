using System.Windows;
using System.Windows.Controls;

namespace EnglishLocker.Converters;

/// <summary>
/// DataTemplateSelector động cho Action Tab:
/// 1. Ưu tiên tìm DataTemplate đã định nghĩa trong Resources.
/// 2. Nếu chưa định nghĩa trước, tự động tìm và ánh xạ View tương ứng theo Convention
///    (ví dụ: MyActionViewModel -> MyActionControl hoặc MyActionView).
/// </summary>
public class TabTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item == null) return null;

        var vmType = item.GetType();

        // 1. Tìm DataTemplate trong Resources nếu đã được khai báo
        if (container is FrameworkElement element)
        {
            var resource = element.TryFindResource(new DataTemplateKey(vmType));
            if (resource is DataTemplate template)
            {
                return template;
            }
        }

        // 2. Tự động tìm View tương ứng theo Convention trong assembly của EnglishLocker
        var viewType = ResolveViewType(vmType);
        if (viewType != null)
        {
            var factory = new FrameworkElementFactory(viewType);
            return new DataTemplate(vmType) { VisualTree = factory };
        }

        return base.SelectTemplate(item, container);
    }

    private static Type? ResolveViewType(Type vmType)
    {
        var assembly = typeof(TabTemplateSelector).Assembly;
        var targetControlName = vmType.Name.Replace("ViewModel", "Control");
        var targetViewName = vmType.Name.Replace("ViewModel", "View");

        foreach (var type in assembly.GetTypes())
        {
            if (type.Name == targetControlName || type.Name == targetViewName)
            {
                if (typeof(FrameworkElement).IsAssignableFrom(type))
                {
                    return type;
                }
            }
        }

        return null;
    }
}
