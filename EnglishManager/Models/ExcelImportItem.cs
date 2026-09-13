namespace EnglishManager.Models;

/// <summary>
/// Đại diện cho một dòng dữ liệu xem trước khi đọc file Excel để import vào LearningMaterial.
/// </summary>
public class ExcelImportItem
{
    public int RowIndex { get; set; }
    public string Term { get; set; } = string.Empty;
    public string? ContextTag { get; set; }
    public string? Phonetics { get; set; }
    public string Meaning { get; set; } = string.Empty;
    public string? EnglishExample { get; set; }
    public string? VietnameseExample { get; set; }
    public string? ExampleSentence { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Term) && !string.IsNullOrWhiteSpace(Meaning);
    public string StatusText => IsValid ? "Hợp lệ" : "Thiếu từ hoặc nghĩa";
}
