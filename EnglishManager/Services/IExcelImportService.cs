using English.Entity.Enums;
using EnglishManager.Models;

namespace EnglishManager.Services;

public interface IExcelImportService
{
    List<ExcelImportItem> ParseExcelFile(string filePath, bool hasHeaderRow);
    Task<int> ImportToDatabaseAsync(IEnumerable<ExcelImportItem> items, CEFRLevel defaultLevel);
}
