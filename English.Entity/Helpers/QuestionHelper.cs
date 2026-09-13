using System.Text.Json;

namespace English.Entity.Helpers;

/// <summary>
/// Helper xử lý serialization và chuyển đổi cho Question options.
/// </summary>
public static class QuestionHelper
{
    public static List<string> ParseOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static string? SerializeOptions(IEnumerable<string>? options)
    {
        if (options == null)
            return null;

        var list = options.ToList();
        return list.Count > 0 ? JsonSerializer.Serialize(list) : null;
    }
}
