using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

public partial class QuestionManagementViewModel : ObservableObject
{
    private readonly IInAppDialogService _dialogService;

    public static readonly List<QuestionDto> MockQuestions = new()
    {
        new QuestionDto
        {
            Id = Guid.NewGuid(),
            LearningMaterialId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            TestType = TestType.ReverseTranslation,
            Prompt = "Nghĩa của từ 'Abandon' là gì?",
            CorrectAnswer = "Từ bỏ, ruồng bỏ",
            Options = new List<string> { "Từ bỏ, ruồng bỏ", "Giữ lại", "Chào đón", "Xây dựng" }
        },
        new QuestionDto
        {
            Id = Guid.NewGuid(),
            LearningMaterialId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            TestType = TestType.ReverseTranslation,
            Prompt = "Nghĩa của từ 'Brilliant' là gì?",
            CorrectAnswer = "Xuất sắc, rực rỡ",
            Options = new List<string> { "Tối tăm", "Xuất sắc, rực rỡ", "Chậm chạp", "Yếu ớt" }
        },
        new QuestionDto
        {
            Id = Guid.NewGuid(),
            LearningMaterialId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            TestType = TestType.ReverseTranslation,
            Prompt = "Nghĩa của từ 'Coherent' là gì?",
            CorrectAnswer = "Mạch lạc, chặt chẽ",
            Options = new List<string> { "Rời rạc", "Mạch lạc, chặt chẽ", "Khó hiểu", "Mơ hồ" }
        }
    };

    [ObservableProperty]
    private ObservableCollection<DeckDto> _decks = new();

    [ObservableProperty]
    private DeckDto? _filterDeck;

    [ObservableProperty]
    private ObservableCollection<LearningMaterialDto> _materials = new();

    [ObservableProperty]
    private LearningMaterialDto? _filterMaterial;

    [ObservableProperty]
    private ObservableCollection<QuestionDto> _allQuestions = new();

    [ObservableProperty]
    private ObservableCollection<QuestionDto> _filteredQuestions = new();

    [ObservableProperty]
    private QuestionDto? _selectedQuestion;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public List<TestType> AvailableTestTypes { get; } = Enum.GetValues<TestType>().ToList();

    // Form fields
    [ObservableProperty]
    private Guid? _editingQuestionId;

    [ObservableProperty]
    private Guid _formMaterialId;

    [ObservableProperty]
    private TestType _formTestType = TestType.ReverseTranslation;

    [ObservableProperty]
    private string _formPrompt = string.Empty;

    [ObservableProperty]
    private string _formCorrectAnswer = string.Empty;

    [ObservableProperty]
    private string _formOptionA = string.Empty;

    [ObservableProperty]
    private string _formOptionB = string.Empty;

    [ObservableProperty]
    private string _formOptionC = string.Empty;

    [ObservableProperty]
    private string _formOptionD = string.Empty;

    [ObservableProperty]
    private string _formAudioLocalPath = string.Empty;

    [ObservableProperty]
    private string _formImageLocalPath = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm câu hỏi mới";

    public QuestionManagementViewModel(IInAppDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnFilterDeckChanged(DeckDto? value)
    {
        _ = OnFilterDeckChangedAsync(value);
    }

    partial void OnFilterMaterialChanged(LearningMaterialDto? value)
    {
        _ = LoadQuestionsForFilterAsync();
    }

    partial void OnSelectedQuestionChanged(QuestionDto? value)
    {
        if (value != null && !IsEditing)
        {
            LoadIntoForm(value, false);
        }
    }

    private async Task OnFilterDeckChangedAsync(DeckDto? deck)
    {
        try
        {
            IsLoading = true;
            await Task.Yield();
            var matList = deck != null
                ? MaterialManagementViewModel.MockMaterials.Where(m => m.DeckId == deck.Id).ToList()
                : MaterialManagementViewModel.MockMaterials.ToList();

            Materials = new ObservableCollection<LearningMaterialDto>(matList);
            FilterMaterial = null;
            await LoadQuestionsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi lọc dữ liệu", $"Không thể tải danh sách tài liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Yield();
            Decks = new ObservableCollection<DeckDto>(DeckManagementViewModel.MockDecks.Select(d => new DeckDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                MaterialCount = d.MaterialCount
            }));

            Materials = new ObservableCollection<LearningMaterialDto>(MaterialManagementViewModel.MockMaterials.Select(m => new LearningMaterialDto
            {
                Id = m.Id,
                DeckId = m.DeckId,
                Term = m.Term,
                Meaning = m.Meaning,
                CategoryType = m.CategoryType,
                ContextTag = m.ContextTag,
                Phonetics = m.Phonetics,
                ExampleSentence = m.ExampleSentence,
                QuestionCount = m.QuestionCount
            }));

            await LoadQuestionsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách câu hỏi: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadQuestionsForFilterAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Yield();
            var materialIds = Materials.Select(m => m.Id).ToHashSet();
            var list = FilterMaterial != null
                ? MockQuestions.Where(q => q.LearningMaterialId == FilterMaterial.Id).ToList()
                : MockQuestions.Where(q => materialIds.Contains(q.LearningMaterialId)).ToList();

            AllQuestions = new ObservableCollection<QuestionDto>(list.Select(q => new QuestionDto
            {
                Id = q.Id,
                LearningMaterialId = q.LearningMaterialId,
                TestType = q.TestType,
                Prompt = q.Prompt,
                CorrectAnswer = q.CorrectAnswer,
                Options = new List<string>(q.Options),
                AudioLocalPath = q.AudioLocalPath,
                ImageLocalPath = q.ImageLocalPath
            }));
            ApplyFilter();

            if (SelectedQuestion != null)
            {
                SelectedQuestion = AllQuestions.FirstOrDefault(q => q.Id == SelectedQuestion.Id);
            }
            else if (AllQuestions.Count > 0)
            {
                SelectedQuestion = AllQuestions[0];
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải câu hỏi", $"Không thể lấy danh sách câu hỏi: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredQuestions = new ObservableCollection<QuestionDto>(AllQuestions);
        }
        else
        {
            var q = SearchText.Trim().ToLowerInvariant();
            var matches = AllQuestions.Where(x =>
                (x.Prompt != null && x.Prompt.ToLowerInvariant().Contains(q)) ||
                (x.CorrectAnswer != null && x.CorrectAnswer.ToLowerInvariant().Contains(q)));
            FilteredQuestions = new ObservableCollection<QuestionDto>(matches);
        }
    }

    [RelayCommand]
    public void StartCreate()
    {
        if (Materials.Count == 0)
        {
            _ = _dialogService.ShowWarningAsync("Chưa có từ vựng", "Bạn cần tạo ít nhất một từ vựng / tài liệu trước khi thêm câu hỏi.");
            return;
        }

        EditingQuestionId = null;
        FormMaterialId = FilterMaterial?.Id ?? Materials[0].Id;
        FormTestType = TestType.ReverseTranslation;
        FormPrompt = string.Empty;
        FormCorrectAnswer = string.Empty;
        FormOptionA = string.Empty;
        FormOptionB = string.Empty;
        FormOptionC = string.Empty;
        FormOptionD = string.Empty;
        FormAudioLocalPath = string.Empty;
        FormImageLocalPath = string.Empty;
        FormTitle = "Thêm câu hỏi mới";
        IsEditing = true;
    }

    [RelayCommand]
    public void StartEdit(QuestionDto? target)
    {
        var item = target ?? SelectedQuestion;
        if (item == null) return;
        LoadIntoForm(item, true);
    }

    private void LoadIntoForm(QuestionDto item, bool enableEdit)
    {
        EditingQuestionId = item.Id;
        FormMaterialId = item.LearningMaterialId;
        FormTestType = item.TestType;
        FormPrompt = item.Prompt;
        FormCorrectAnswer = item.CorrectAnswer;

        FormOptionA = item.Options.Count > 0 ? item.Options[0] : string.Empty;
        FormOptionB = item.Options.Count > 1 ? item.Options[1] : string.Empty;
        FormOptionC = item.Options.Count > 2 ? item.Options[2] : string.Empty;
        FormOptionD = item.Options.Count > 3 ? item.Options[3] : string.Empty;

        FormAudioLocalPath = item.AudioLocalPath ?? string.Empty;
        FormImageLocalPath = item.ImageLocalPath ?? string.Empty;
        FormTitle = $"Chỉnh sửa câu hỏi: {item.Prompt}";
        IsEditing = enableEdit;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FormPrompt))
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng nhập nội dung / đề bài câu hỏi (Prompt).");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormCorrectAnswer))
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng nhập đáp án chính xác (Correct Answer).");
            return;
        }

        if (FormMaterialId == Guid.Empty)
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng chọn từ vựng tương ứng cho câu hỏi.");
            return;
        }

        var options = new List<string>();
        if (!string.IsNullOrWhiteSpace(FormOptionA)) options.Add(FormOptionA.Trim());
        if (!string.IsNullOrWhiteSpace(FormOptionB)) options.Add(FormOptionB.Trim());
        if (!string.IsNullOrWhiteSpace(FormOptionC)) options.Add(FormOptionC.Trim());
        if (!string.IsNullOrWhiteSpace(FormOptionD)) options.Add(FormOptionD.Trim());

        // Đảm bảo đáp án đúng có mặt trong danh sách options nếu đây là câu hỏi trắc nghiệm và có nhập options
        if (options.Count > 0 && !options.Any(o => string.Equals(o, FormCorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            options.Insert(0, FormCorrectAnswer.Trim());
        }

        try
        {
            IsLoading = true;
            await Task.Yield();
            if (EditingQuestionId == null)
            {
                var dto = new QuestionDto
                {
                    Id = Guid.NewGuid(),
                    LearningMaterialId = FormMaterialId,
                    TestType = FormTestType,
                    Prompt = FormPrompt.Trim(),
                    CorrectAnswer = FormCorrectAnswer.Trim(),
                    Options = options,
                    AudioLocalPath = string.IsNullOrWhiteSpace(FormAudioLocalPath) ? null : FormAudioLocalPath.Trim(),
                    ImageLocalPath = string.IsNullOrWhiteSpace(FormImageLocalPath) ? null : FormImageLocalPath.Trim()
                };
                MockQuestions.Add(dto);
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã thêm mới câu hỏi cho từ vựng.");
            }
            else
            {
                var existing = MockQuestions.FirstOrDefault(q => q.Id == EditingQuestionId.Value);
                if (existing != null)
                {
                    existing.LearningMaterialId = FormMaterialId;
                    existing.TestType = FormTestType;
                    existing.Prompt = FormPrompt.Trim();
                    existing.CorrectAnswer = FormCorrectAnswer.Trim();
                    existing.Options = options;
                    existing.AudioLocalPath = string.IsNullOrWhiteSpace(FormAudioLocalPath) ? null : FormAudioLocalPath.Trim();
                    existing.ImageLocalPath = string.IsNullOrWhiteSpace(FormImageLocalPath) ? null : FormImageLocalPath.Trim();
                }
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã cập nhật câu hỏi.");
            }

            IsEditing = false;
            await LoadQuestionsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi lưu dữ liệu", $"Thao tác lưu câu hỏi thất bại: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditing = false;
        if (SelectedQuestion != null)
        {
            LoadIntoForm(SelectedQuestion, false);
        }
    }

    [RelayCommand]
    public async Task DeleteAsync(QuestionDto? target)
    {
        var item = target ?? SelectedQuestion;
        if (item == null) return;

        bool confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa câu hỏi",
            $"Bạn có chắc chắn muốn xóa câu hỏi:\n\"{item.Prompt}\"?");

        if (!confirmed) return;

        try
        {
            IsLoading = true;
            await Task.Yield();
            MockQuestions.RemoveAll(q => q.Id == item.Id);
            await _dialogService.ShowSuccessAsync("Đã xóa", "Câu hỏi đã được xóa thành công.");
            await LoadQuestionsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi xóa câu hỏi", $"Không thể xóa câu hỏi: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
