namespace EnglishManager.Models;

/// <summary>
/// Model ánh xạ phản hồi JSON từ AI Dictionary API (http://localhost:8000).
/// Sử dụng System.Text.Json với PropertyNameCaseInsensitive = true.
/// </summary>
public class AiDictionaryResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AiVocabularyData? Data { get; set; }
}

/// <summary>
/// Dữ liệu từ vựng trả về từ API — tầng 1 (Vocabulary).
/// </summary>
public class AiVocabularyData
{
    public string WordText { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phonetic_UK { get; set; }
    public string? Phonetic_US { get; set; }
    public string? AudioPath_UK { get; set; }
    public string? AudioPath_US { get; set; }
    public string? Level { get; set; }
    public List<string> WordFamily { get; set; } = new();
    public List<AiMeaningData> Meanings { get; set; } = new();
}

/// <summary>
/// Dữ liệu ngữ nghĩa trả về từ API — tầng 2 (VocabularyMeaning).
/// </summary>
public class AiMeaningData
{
    public string? WordClass { get; set; }
    public string? Definition_EN { get; set; }
    public string? Definition_VI { get; set; }
    public string? Context { get; set; }
    public List<string> Synonyms { get; set; } = new();
    public List<string> Antonyms { get; set; } = new();
    public List<AiExampleData> Examples { get; set; } = new();
}

/// <summary>
/// Dữ liệu câu ví dụ trả về từ API — tầng 3 (MeaningExample).
/// </summary>
public class AiExampleData
{
    public string? Sentence_EN { get; set; }
    public string? Sentence_VI { get; set; }
    public string? HighlightedTarget { get; set; }
}
