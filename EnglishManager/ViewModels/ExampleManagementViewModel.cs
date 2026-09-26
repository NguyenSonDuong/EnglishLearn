using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Services;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

/// <summary>
/// Item hiển thị ngữ nghĩa cha kèm từ vựng phục vụ ComboBox liên kết.
/// </summary>
public class MeaningLookupItem
{
    public Guid MeaningId { get; set; }
    public Guid VocabularyId { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel quản lý danh sách và các thao tác CRUD câu ví dụ (MeaningExample).
/// Có ComboBox chọn ngữ nghĩa cha liên kết bảng.
/// </summary>
public partial class ExampleManagementViewModel : ObservableObject
{
    private readonly IMeaningExampleService _exampleService;
    private readonly IVocabularyMeaningService _meaningService;
    private readonly IVocabularyService _vocabService;
    private readonly IInAppDialogService _dialogService;

    // ── Observable Collections ──
    [ObservableProperty]
    private ObservableCollection<MeaningExampleDto> _allExamples = new();

    [ObservableProperty]
    private ObservableCollection<MeaningExampleDto> _filteredExamples = new();

    [ObservableProperty]
    private MeaningExampleDto? _selectedExample;

    // Danh sách ngữ nghĩa cha phục vụ ComboBox liên kết
    [ObservableProperty]
    private ObservableCollection<MeaningLookupItem> _meanings = new();

    // ── Filter State ──
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Guid? _filterMeaningId;

    [ObservableProperty]
    private bool _isLoading;

    // ── Form Modal In-App State ──
    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm Câu Ví Dụ Mới";

    [ObservableProperty]
    private Guid _formId;

    // ComboBox liên kết bảng cha: MeaningId
    [ObservableProperty]
    private Guid _formMeaningId;

    [ObservableProperty]
    private string _formSentenceEN = string.Empty;

    [ObservableProperty]
    private string _formSentenceVI = string.Empty;

    [ObservableProperty]
    private string _formHighlightedTarget = string.Empty;

    public ExampleManagementViewModel(
        IMeaningExampleService exampleService,
        IVocabularyMeaningService meaningService,
        IVocabularyService vocabService,
        IInAppDialogService dialogService)
    {
        _exampleService = exampleService;
        _meaningService = meaningService;
        _vocabService = vocabService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var vocabList = await _vocabService.GetAllVocabulariesAsync();
            var vocabDict = vocabList.ToDictionary(v => v.Id, v => v.WordText);

            var meaningList = await _meaningService.GetAllMeaningsAsync();
            var lookupItems = meaningList.Select(m =>
            {
                var word = vocabDict.TryGetValue(m.VocabularyId, out var w) ? w : "Không rõ";
                return new MeaningLookupItem
                {
                    MeaningId = m.Id,
                    VocabularyId = m.VocabularyId,
                    DisplayText = $"[{word}] ({m.WordClass}) {m.Definition_VI}"
                };
            }).OrderBy(x => x.DisplayText).ToList();

            Meanings = new ObservableCollection<MeaningLookupItem>(lookupItems);

            var exampleList = await _exampleService.GetAllExamplesAsync();
            AllExamples = new ObservableCollection<MeaningExampleDto>(exampleList);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách câu ví dụ:\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnFilterMeaningIdChanged(Guid? value) => ApplyFilter();

    public void ApplyFilter()
    {
        var query = AllExamples.AsEnumerable();

        if (FilterMeaningId.HasValue && FilterMeaningId.Value != Guid.Empty)
        {
            query = query.Where(e => e.MeaningId == FilterMeaningId.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            query = query.Where(e => e.Sentence_EN.ToLowerInvariant().Contains(search)
                                  || e.Sentence_VI.ToLowerInvariant().Contains(search)
                                  || (e.HighlightedTarget != null && e.HighlightedTarget.ToLowerInvariant().Contains(search)));
        }

        FilteredExamples = new ObservableCollection<MeaningExampleDto>(query);
    }

    [RelayCommand]
    public void ResetFilter()
    {
        SearchText = string.Empty;
        FilterMeaningId = null;
        ApplyFilter();
    }

    public string GetMeaningDisplayText(Guid meaningId)
    {
        var item = Meanings.FirstOrDefault(m => m.MeaningId == meaningId);
        return item?.DisplayText ?? "Không xác định";
    }

    // ── CRUD Form Actions ──

    [RelayCommand]
    public void OpenCreateForm(Guid? preselectedMeaningId = null)
    {
        IsEditing = false;
        FormTitle = "Thêm Câu Ví Dụ Mới";
        FormId = Guid.Empty;

        FormMeaningId = preselectedMeaningId ?? FilterMeaningId ?? Meanings.FirstOrDefault()?.MeaningId ?? Guid.Empty;
        FormSentenceEN = string.Empty;
        FormSentenceVI = string.Empty;
        FormHighlightedTarget = string.Empty;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void StartEdit(MeaningExampleDto? item)
    {
        if (item == null) return;

        IsEditing = true;
        FormTitle = "Chỉnh Sửa Câu Ví Dụ";
        FormId = item.Id;
        FormMeaningId = item.MeaningId;
        FormSentenceEN = item.Sentence_EN;
        FormSentenceVI = item.Sentence_VI;
        FormHighlightedTarget = item.HighlightedTarget ?? string.Empty;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void CloseForm()
    {
        IsFormOpen = false;
    }

    [RelayCommand]
    public async Task SaveFormAsync()
    {
        if (FormMeaningId == Guid.Empty)
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng chọn Ngữ nghĩa cha (Meaning) từ danh sách.");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormSentenceEN))
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng nhập Câu ví dụ Tiếng Anh (Sentence_EN).");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormSentenceVI))
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng nhập Dịch nghĩa Tiếng Việt (Sentence_VI).");
            return;
        }

        var dto = new MeaningExampleDto
        {
            Id = IsEditing ? FormId : Guid.NewGuid(),
            MeaningId = FormMeaningId,
            Sentence_EN = FormSentenceEN.Trim(),
            Sentence_VI = FormSentenceVI.Trim(),
            HighlightedTarget = string.IsNullOrWhiteSpace(FormHighlightedTarget) ? null : FormHighlightedTarget.Trim()
        };

        IsLoading = true;
        try
        {
            if (IsEditing)
            {
                var success = await _exampleService.UpdateExampleAsync(dto);
                if (success)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", "Đã cập nhật câu ví dụ thành công.");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể cập nhật câu ví dụ.");
                }
            }
            else
            {
                var created = await _exampleService.CreateExampleAsync(dto);
                if (created != null)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", "Đã thêm câu ví dụ thành công.");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể thêm câu ví dụ.");
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi lưu dữ liệu", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteAsync(MeaningExampleDto? item)
    {
        if (item == null) return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa câu ví dụ",
            $"Bạn có chắc chắn muốn xóa câu ví dụ:\n\"{item.Sentence_EN}\"?");

        if (!confirmed) return;

        IsLoading = true;
        try
        {
            var success = await _exampleService.DeleteExampleAsync(item.Id);
            if (success)
            {
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Thành công", "Đã xóa câu ví dụ thành công.");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Thất bại", "Không thể xóa câu ví dụ đã chọn.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi xóa dữ liệu", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
