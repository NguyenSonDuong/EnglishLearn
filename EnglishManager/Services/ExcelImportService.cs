using System.IO;
using System.Text;
using ExcelDataReader;
using English.Entity.Enums;
using English.Repository.Data;
using EnglishManager.Models;

namespace EnglishManager.Services;

public class ExcelImportService : IExcelImportService
{
    private readonly AppDbContext _dbContext;

    public ExcelImportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        // Đăng ký CodePagesEncodingProvider để hỗ trợ bảng mã tiếng Việt và các định dạng Excel
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public List<ExcelImportItem> ParseExcelFile(string filePath, bool hasHeaderRow)
    {
        var result = new List<ExcelImportItem>();

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        int currentRow = 0;
        while (reader.Read())
        {
            currentRow++;
            if (hasHeaderRow && currentRow == 1)
            {
                continue;
            }

            int fieldCount = reader.FieldCount;
            if (fieldCount == 0) continue;

            string? term = fieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
            string? contextTag = fieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
            string? phonetics = fieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;
            string? meaning = fieldCount > 3 ? reader.GetValue(3)?.ToString()?.Trim() : null;
            string? exEn = fieldCount > 4 ? reader.GetValue(4)?.ToString()?.Trim() : null;
            string? exVi = fieldCount > 5 ? reader.GetValue(5)?.ToString()?.Trim() : null;

            // Bỏ qua nếu dòng hoàn toàn rỗng
            if (string.IsNullOrWhiteSpace(term) &&
                string.IsNullOrWhiteSpace(contextTag) &&
                string.IsNullOrWhiteSpace(phonetics) &&
                string.IsNullOrWhiteSpace(meaning) &&
                string.IsNullOrWhiteSpace(exEn) &&
                string.IsNullOrWhiteSpace(exVi))
            {
                continue;
            }

            // Ghép cột 5 và 6 thành 1 chuỗi đưa vào ExampleSentence
            string? mergedExample = null;
            if (!string.IsNullOrWhiteSpace(exEn) && !string.IsNullOrWhiteSpace(exVi))
            {
                mergedExample = $"{exEn}\n{exVi}";
            }
            else if (!string.IsNullOrWhiteSpace(exEn))
            {
                mergedExample = exEn;
            }
            else if (!string.IsNullOrWhiteSpace(exVi))
            {
                mergedExample = exVi;
            }

            result.Add(new ExcelImportItem
            {
                RowIndex = result.Count + 1,
                Term = term ?? string.Empty,
                ContextTag = contextTag,
                Phonetics = phonetics,
                Meaning = meaning ?? string.Empty,
                EnglishExample = exEn,
                VietnameseExample = exVi,
                ExampleSentence = mergedExample
            });
        }

        return result;
    }

    public Task<int> ImportToDatabaseAsync(IEnumerable<ExcelImportItem> items, Guid deckId, CategoryType categoryType)
    {
        var validItems = items.Where(i => i.IsValid).ToList();
        return Task.FromResult(validItems.Count);
    }
}
