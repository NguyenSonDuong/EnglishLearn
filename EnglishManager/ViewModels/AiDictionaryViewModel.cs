using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;
using EnglishManager.Models;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

/// <summary>
/// ViewModel quản lý toàn bộ state cho Tab 3 "AI Dictionary".
/// Hỗ trợ 2 mode: Sinh từ ngẫu nhiên theo Level, và Tra cứu từ tiếng Anh.
/// Khi có kết quả, cho phép lưu 3 tầng vào SQLite qua các Service hiện có.
/// </summary>
public partial class AiDictionaryViewModel : ObservableObject
{
    private readonly IAiDictionaryApiService _apiService;
    private readonly IVocabularyService _vocabService;
    private readonly IVocabularyMeaningService _meaningService;
    private readonly IMeaningExampleService _exampleService;
    private readonly IInAppDialogService _dialogService;

    // ── Mode Toggle ──
    [ObservableProperty]
    private bool _isGenerateMode = true;

    [ObservableProperty]
    private bool _isLookupMode = false;

    // ── Generate Controls ──
    [ObservableProperty]
    private string _selectedLevel = "Random";

    public List<string> AvailableLevels { get; } = ["Random", "A1", "A2", "B1", "B2", "C1", "C2"];

    // ── Lookup Controls ──
    [ObservableProperty]
    private string _lookupWord = string.Empty;

    // ── State ──
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasResult;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ── Result Preview (tầng 1 chứa cả Meanings và Examples) ──
    [ObservableProperty]
    private AiVocabularyData? _currentResult;

    public AiDictionaryViewModel(
        IAiDictionaryApiService apiService,
        IVocabularyService vocabService,
        IVocabularyMeaningService meaningService,
        IMeaningExampleService exampleService,
        IInAppDialogService dialogService)
    {
        _apiService = apiService;
        _vocabService = vocabService;
        _meaningService = meaningService;
        _exampleService = exampleService;
        _dialogService = dialogService;
    }

    // ── Mode Switching ──

    [RelayCommand]
    public void SwitchToGenerate()
    {
        IsGenerateMode = true;
        IsLookupMode = false;
        ClearResult();
    }

    [RelayCommand]
    public void SwitchToLookup()
    {
        IsGenerateMode = false;
        IsLookupMode = true;
        ClearResult();
    }

    private void ClearResult()
    {
        CurrentResult = null;
        HasResult = false;
        StatusMessage = string.Empty;
    }

    // ── API Calls ──

    [RelayCommand]
    public async Task GenerateAsync()
    {
        await ExecuteApiCallAsync(async () =>
        {
            var existingVocabs = await _vocabService.GetAllVocabulariesAsync();
            var excludeWords = existingVocabs
                .Select(v => v.WordText)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return await _apiService.GenerateByLevelAsync(SelectedLevel, excludeWords);
        });
    }

    [RelayCommand]
    public async Task LookupAsync()
    {
        if (string.IsNullOrWhiteSpace(LookupWord))
        {
            StatusMessage = "⚠️ Vui lòng nhập từ cần tra cứu.";
            return;
        }
        await ExecuteApiCallAsync(() => _apiService.LookupWordAsync(LookupWord));
    }

    private async Task ExecuteApiCallAsync(Func<Task<AiDictionaryResponse?>> apiCall)
    {
        IsLoading = true;
        HasResult = false;
        CurrentResult = null;
        StatusMessage = "⏳ Đang kết nối AI, vui lòng chờ...";

        try
        {
            var response = await apiCall();
            if (response is { Success: true, Data: not null })
            {
                CurrentResult = response.Data;
                HasResult = true;
                int totalExamples = response.Data.Meanings.Sum(m => m.Examples.Count);
                StatusMessage = $"✅ Đã tìm thấy: \"{response.Data.WordText}\"  —  {response.Data.Meanings.Count} ngữ nghĩa, {totalExamples} câu ví dụ";
            }
            else
            {
                StatusMessage = $"⚠️ {response?.Message ?? "API không trả về dữ liệu hợp lệ."}";
            }
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = $"❌ Lỗi kết nối API: {ex.Message}\nHãy đảm bảo server đang chạy tại http://localhost:8000";
        }
        catch (TaskCanceledException)
        {
            StatusMessage = "❌ Yêu cầu hết thời gian chờ (timeout). Server có thể đang xử lý quá lâu.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Lỗi không xác định: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Save to Database ──

    [RelayCommand]
    public async Task SaveToDatabaseAsync()
    {
        if (CurrentResult == null) return;

        // Kiểm tra từ đã tồn tại trong database chưa
        var allVocabs = await _vocabService.GetAllVocabulariesAsync();
        bool alreadyExists = allVocabs.Any(v =>
            string.Equals(v.WordText, CurrentResult.WordText, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            await _dialogService.ShowWarningAsync(
                "Từ đã tồn tại",
                $"Từ vựng \"{CurrentResult.WordText}\" đã có trong từ điển cá nhân.\nVui lòng kiểm tra lại ở Tab \"Từ Vựng (Vocabulary)\".");
            return;
        }

        IsLoading = true;
        try
        {
            // ── Bước 1: Lưu Vocabulary (Tầng 1) ──
            var vocabDto = MapToVocabularyDto(CurrentResult);
            var createdVocab = await _vocabService.CreateVocabularyAsync(vocabDto);
            if (createdVocab == null)
                throw new Exception("Không thể tạo từ vựng (CreateVocabularyAsync trả về null).");

            int savedMeanings = 0;
            int savedExamples = 0;

            // ── Bước 2 & 3: Lưu VocabularyMeaning (Tầng 2) + MeaningExample (Tầng 3) ──
            foreach (var meaning in CurrentResult.Meanings)
            {
                var meaningDto = MapToMeaningDto(meaning, createdVocab.Id);
                var createdMeaning = await _meaningService.CreateMeaningAsync(meaningDto);
                if (createdMeaning == null) continue;
                savedMeanings++;

                foreach (var example in meaning.Examples)
                {
                    var exampleDto = MapToExampleDto(example, createdMeaning.Id);
                    var createdExample = await _exampleService.CreateExampleAsync(exampleDto);
                    if (createdExample != null) savedExamples++;
                }
            }

            await _dialogService.ShowSuccessAsync(
                "Lưu thành công! 🎉",
                $"Đã lưu \"{createdVocab.WordText}\" (Cấp độ {createdVocab.Level}) vào từ điển cá nhân:\n" +
                $"• {savedMeanings} ngữ nghĩa\n" +
                $"• {savedExamples} câu ví dụ");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi lưu dữ liệu", $"Không thể lưu từ vựng vào database:\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Private Mapping Helpers ──

    private static VocabularyDto MapToVocabularyDto(AiVocabularyData data) => new()
    {
        Id = Guid.NewGuid(),
        WordText = data.WordText.Trim(),
        Description = string.IsNullOrWhiteSpace(data.Description) ? null : data.Description.Trim(),
        Phonetic_UK = string.IsNullOrWhiteSpace(data.Phonetic_UK) ? null : data.Phonetic_UK.Trim(),
        Phonetic_US = string.IsNullOrWhiteSpace(data.Phonetic_US) ? null : data.Phonetic_US.Trim(),
        AudioPath_UK = string.IsNullOrWhiteSpace(data.AudioPath_UK) ? null : data.AudioPath_UK.Trim(),
        AudioPath_US = string.IsNullOrWhiteSpace(data.AudioPath_US) ? null : data.AudioPath_US.Trim(),
        Level = ParseCefrLevel(data.Level),
        WordFamily = data.WordFamily ?? new List<string>()
    };

    private static VocabularyMeaningDto MapToMeaningDto(AiMeaningData meaning, Guid vocabularyId) => new()
    {
        Id = Guid.NewGuid(),
        VocabularyId = vocabularyId,
        WordClass = ParseWordClass(meaning.WordClass),
        Definition_EN = meaning.Definition_EN ?? string.Empty,
        Definition_VI = meaning.Definition_VI ?? string.Empty,
        Context = ParseContextTag(meaning.Context),
        Synonyms = meaning.Synonyms ?? new List<string>(),
        Antonyms = meaning.Antonyms ?? new List<string>()
    };

    private static MeaningExampleDto MapToExampleDto(AiExampleData example, Guid meaningId) => new()
    {
        Id = Guid.NewGuid(),
        MeaningId = meaningId,
        Sentence_EN = example.Sentence_EN ?? string.Empty,
        Sentence_VI = example.Sentence_VI ?? string.Empty,
        HighlightedTarget = string.IsNullOrWhiteSpace(example.HighlightedTarget) ? null : example.HighlightedTarget.Trim()
    };

    // ── Enum Parsers (string từ API → Enum trong Entity) ──

    private static CEFRLevel ParseCefrLevel(string? level) => level?.Trim().ToUpperInvariant() switch
    {
        "A1" => CEFRLevel.A1,
        "A2" => CEFRLevel.A2,
        "B1" => CEFRLevel.B1,
        "B2" => CEFRLevel.B2,
        "C1" => CEFRLevel.C1,
        "C2" => CEFRLevel.C2,
        _ => CEFRLevel.Uncategorized
    };

    private static WordClass ParseWordClass(string? wc) => wc?.Trim().ToLowerInvariant() switch
    {
        "noun" => WordClass.Noun,
        "verb" => WordClass.Verb,
        "adjective" => WordClass.Adjective,
        "adverb" => WordClass.Adverb,
        "pronoun" => WordClass.Pronoun,
        "preposition" => WordClass.Preposition,
        "conjunction" => WordClass.Conjunction,
        "interjection" => WordClass.Interjection,
        "phrasalverb" or "phrasal verb" or "phrasal-verb" => WordClass.PhrasalVerb,
        "idiom" => WordClass.Idiom,
        _ => WordClass.Noun
    };

    private static ContextTag ParseContextTag(string? ctx) => ctx?.Trim().ToLowerInvariant() switch
    {
        "general" => ContextTag.General,
        "formal" => ContextTag.Formal,
        "informal" => ContextTag.Informal,
        "slang" => ContextTag.Slang,
        "business" => ContextTag.Business,
        "it" => ContextTag.IT,
        "medical" => ContextTag.Medical,
        _ => ContextTag.General
    };
}
