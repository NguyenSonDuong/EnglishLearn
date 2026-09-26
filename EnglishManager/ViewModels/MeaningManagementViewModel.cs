using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

/// <summary>
/// ViewModel quản lý danh sách và các thao tác CRUD ngữ nghĩa từ vựng (VocabularyMeaning).
/// Có ComboBox chọn từ vựng cha liên kết bảng.
/// </summary>
public partial class MeaningManagementViewModel : ObservableObject
{
    private readonly IVocabularyMeaningService _meaningService;
    private readonly IVocabularyService _vocabService;
    private readonly IInAppDialogService _dialogService;

    // ── Observable Collections ──
    [ObservableProperty]
    private ObservableCollection<VocabularyMeaningDto> _allMeanings = new();

    [ObservableProperty]
    private ObservableCollection<VocabularyMeaningDto> _filteredMeanings = new();

    [ObservableProperty]
    private VocabularyMeaningDto? _selectedMeaning;

    // Danh sách từ vựng cha phục vụ ComboBox liên kết
    [ObservableProperty]
    private ObservableCollection<VocabularyDto> _vocabularies = new();

    // ── Filter State ──
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Guid? _filterVocabularyId;

    [ObservableProperty]
    private WordClass? _filterWordClass;

    [ObservableProperty]
    private bool _isLoading;

    // ── Form Modal In-App State ──
    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm Ngữ Nghĩa Mới";

    [ObservableProperty]
    private Guid _formId;

    // ComboBox liên kết bảng cha: VocabularyId
    [ObservableProperty]
    private Guid _formVocabularyId;

    [ObservableProperty]
    private WordClass _formWordClass = WordClass.Noun;

    [ObservableProperty]
    private ContextTag _formContext = ContextTag.General;

    [ObservableProperty]
    private string _formDefinitionEN = string.Empty;

    [ObservableProperty]
    private string _formDefinitionVI = string.Empty;

    [ObservableProperty]
    private string _formSynonymsText = string.Empty;

    [ObservableProperty]
    private string _formAntonymsText = string.Empty;

    public List<WordClass> AvailableWordClasses { get; } = Enum.GetValues<WordClass>().ToList();
    public List<ContextTag> AvailableContextTags { get; } = Enum.GetValues<ContextTag>().ToList();

    public MeaningManagementViewModel(
        IVocabularyMeaningService meaningService,
        IVocabularyService vocabService,
        IInAppDialogService dialogService)
    {
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
            Vocabularies = new ObservableCollection<VocabularyDto>(vocabList.OrderBy(v => v.WordText));

            var meaningList = await _meaningService.GetAllMeaningsAsync();
            AllMeanings = new ObservableCollection<VocabularyMeaningDto>(meaningList);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách ngữ nghĩa:\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnFilterVocabularyIdChanged(Guid? value) => ApplyFilter();
    partial void OnFilterWordClassChanged(WordClass? value) => ApplyFilter();

    public void ApplyFilter()
    {
        var query = AllMeanings.AsEnumerable();

        if (FilterVocabularyId.HasValue && FilterVocabularyId.Value != Guid.Empty)
        {
            query = query.Where(m => m.VocabularyId == FilterVocabularyId.Value);
        }

        if (FilterWordClass.HasValue)
        {
            query = query.Where(m => m.WordClass == FilterWordClass.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            query = query.Where(m => m.Definition_EN.ToLowerInvariant().Contains(search)
                                  || m.Definition_VI.ToLowerInvariant().Contains(search)
                                  || m.Synonyms.Any(s => s.ToLowerInvariant().Contains(search))
                                  || m.Antonyms.Any(a => a.ToLowerInvariant().Contains(search)));
        }

        FilteredMeanings = new ObservableCollection<VocabularyMeaningDto>(query);
    }

    [RelayCommand]
    public void ResetFilter()
    {
        SearchText = string.Empty;
        FilterVocabularyId = null;
        FilterWordClass = null;
        ApplyFilter();
    }

    public string GetVocabularyWordText(Guid vocabId)
    {
        var vocab = Vocabularies.FirstOrDefault(v => v.Id == vocabId);
        return vocab?.WordText ?? "Không xác định";
    }

    // ── CRUD Form Actions ──

    [RelayCommand]
    public void OpenCreateForm(Guid? preselectedVocabId = null)
    {
        IsEditing = false;
        FormTitle = "Thêm Ngữ Nghĩa Mới";
        FormId = Guid.Empty;

        // Chọn mặc định từ vựng được chỉ định hoặc từ đầu tiên
        FormVocabularyId = preselectedVocabId ?? FilterVocabularyId ?? Vocabularies.FirstOrDefault()?.Id ?? Guid.Empty;
        FormWordClass = WordClass.Noun;
        FormContext = ContextTag.General;
        FormDefinitionEN = string.Empty;
        FormDefinitionVI = string.Empty;
        FormSynonymsText = string.Empty;
        FormAntonymsText = string.Empty;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void StartEdit(VocabularyMeaningDto? item)
    {
        if (item == null) return;

        IsEditing = true;
        var parentWord = GetVocabularyWordText(item.VocabularyId);
        FormTitle = $"Chỉnh Sửa Ngữ Nghĩa của: \"{parentWord}\"";
        FormId = item.Id;
        FormVocabularyId = item.VocabularyId;
        FormWordClass = item.WordClass;
        FormContext = item.Context;
        FormDefinitionEN = item.Definition_EN;
        FormDefinitionVI = item.Definition_VI;
        FormSynonymsText = string.Join(", ", item.Synonyms);
        FormAntonymsText = string.Join(", ", item.Antonyms);

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
        if (FormVocabularyId == Guid.Empty)
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng chọn Từ vựng cha (Vocabulary) từ danh sách.");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormDefinitionEN))
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng nhập Định nghĩa Tiếng Anh (Definition_EN).");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormDefinitionVI))
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng nhập Định nghĩa Tiếng Việt (Definition_VI).");
            return;
        }

        var synList = FormSynonymsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var antList = FormAntonymsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var dto = new VocabularyMeaningDto
        {
            Id = IsEditing ? FormId : Guid.NewGuid(),
            VocabularyId = FormVocabularyId,
            WordClass = FormWordClass,
            Context = FormContext,
            Definition_EN = FormDefinitionEN.Trim(),
            Definition_VI = FormDefinitionVI.Trim(),
            Synonyms = synList,
            Antonyms = antList
        };

        IsLoading = true;
        try
        {
            if (IsEditing)
            {
                var success = await _meaningService.UpdateMeaningAsync(dto);
                if (success)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", "Đã cập nhật ngữ nghĩa thành công.");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể cập nhật ngữ nghĩa.");
                }
            }
            else
            {
                var created = await _meaningService.CreateMeaningAsync(dto);
                if (created != null)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", "Đã tạo mới ngữ nghĩa thành công.");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể thêm mới ngữ nghĩa.");
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
    public async Task DeleteAsync(VocabularyMeaningDto? item)
    {
        if (item == null) return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa ngữ nghĩa",
            $"Bạn có chắc chắn muốn xóa ngữ nghĩa này?\n\"{item.Definition_VI}\"\nLưu ý: Toàn bộ câu ví dụ thuộc ngữ nghĩa này cũng sẽ bị xóa vĩnh viễn!");

        if (!confirmed) return;

        IsLoading = true;
        try
        {
            var success = await _meaningService.DeleteMeaningAsync(item.Id);
            if (success)
            {
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Thành công", "Đã xóa ngữ nghĩa thành công.");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Thất bại", "Không thể xóa ngữ nghĩa đã chọn.");
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
