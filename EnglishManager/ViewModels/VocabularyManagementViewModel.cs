using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

/// <summary>
/// ViewModel quản lý danh sách và các thao tác CRUD từ vựng (Vocabulary).
/// </summary>
public partial class VocabularyManagementViewModel : ObservableObject
{
    private readonly IVocabularyService _vocabService;
    private readonly IInAppDialogService _dialogService;

    // ── Observable Collections ──
    [ObservableProperty]
    private ObservableCollection<VocabularyDto> _allVocabularies = new();

    [ObservableProperty]
    private ObservableCollection<VocabularyDto> _filteredVocabularies = new();

    [ObservableProperty]
    private VocabularyDto? _selectedVocabulary;

    // ── Filter & Search State ──
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private CEFRLevel? _filterLevel;

    [ObservableProperty]
    private bool _isLoading;

    // ── Form Modal In-App State ──
    [ObservableProperty]
    private bool _isFormOpen;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm Từ Vựng Mới";

    [ObservableProperty]
    private Guid _formId;

    [ObservableProperty]
    private string _formWordText = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private string _formPhoneticUK = string.Empty;

    [ObservableProperty]
    private string _formPhoneticUS = string.Empty;

    [ObservableProperty]
    private string _formAudioPathUK = string.Empty;

    [ObservableProperty]
    private string _formAudioPathUS = string.Empty;

    [ObservableProperty]
    private CEFRLevel _formLevel = CEFRLevel.Uncategorized;

    [ObservableProperty]
    private string _formWordFamilyText = string.Empty;

    // Danh sách cấp độ phục vụ ComboBox
    public List<CEFRLevel> AvailableLevels { get; } = Enum.GetValues<CEFRLevel>().ToList();

    public VocabularyManagementViewModel(
        IVocabularyService vocabService,
        IInAppDialogService dialogService)
    {
        _vocabService = vocabService;
        _dialogService = dialogService;
    }

    // ── Data Loading & Filtering ──

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _vocabService.GetAllVocabulariesAsync();
            AllVocabularies = new ObservableCollection<VocabularyDto>(list);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách từ vựng:\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnFilterLevelChanged(CEFRLevel? value) => ApplyFilter();

    public void ApplyFilter()
    {
        var query = AllVocabularies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            query = query.Where(v => v.WordText.ToLowerInvariant().Contains(search)
                                  || (!string.IsNullOrEmpty(v.Description) && v.Description.ToLowerInvariant().Contains(search))
                                  || (v.Phonetic_UK != null && v.Phonetic_UK.ToLowerInvariant().Contains(search))
                                  || (v.Phonetic_US != null && v.Phonetic_US.ToLowerInvariant().Contains(search))
                                  || v.WordFamily.Any(w => w.ToLowerInvariant().Contains(search)));
        }

        if (FilterLevel.HasValue && FilterLevel.Value != CEFRLevel.Uncategorized)
        {
            query = query.Where(v => v.Level == FilterLevel.Value);
        }

        FilteredVocabularies = new ObservableCollection<VocabularyDto>(query.OrderBy(v => v.WordText));
    }

    [RelayCommand]
    public void ResetFilter()
    {
        SearchText = string.Empty;
        FilterLevel = null;
        ApplyFilter();
    }

    // ── CRUD Form Actions ──

    [RelayCommand]
    public void OpenCreateForm()
    {
        IsEditing = false;
        FormTitle = "Thêm Từ Vựng Mới";
        FormId = Guid.Empty;
        FormWordText = string.Empty;
        FormDescription = string.Empty;
        FormPhoneticUK = string.Empty;
        FormPhoneticUS = string.Empty;
        FormAudioPathUK = string.Empty;
        FormAudioPathUS = string.Empty;
        FormLevel = CEFRLevel.A1;
        FormWordFamilyText = string.Empty;

        IsFormOpen = true;
    }

    [RelayCommand]
    public void StartEdit(VocabularyDto? item)
    {
        if (item == null) return;

        IsEditing = true;
        FormTitle = $"Chỉnh Sửa Từ Vựng: \"{item.WordText}\"";
        FormId = item.Id;
        FormWordText = item.WordText;
        FormDescription = item.Description ?? string.Empty;
        FormPhoneticUK = item.Phonetic_UK ?? string.Empty;
        FormPhoneticUS = item.Phonetic_US ?? string.Empty;
        FormAudioPathUK = item.AudioPath_UK ?? string.Empty;
        FormAudioPathUS = item.AudioPath_US ?? string.Empty;
        FormLevel = item.Level;
        FormWordFamilyText = string.Join(", ", item.WordFamily);

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
        if (string.IsNullOrWhiteSpace(FormWordText))
        {
            await _dialogService.ShowWarningAsync("Thông tin thiếu", "Vui lòng nhập Từ vựng (WordText).");
            return;
        }

        var familyList = FormWordFamilyText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var dto = new VocabularyDto
        {
            Id = IsEditing ? FormId : Guid.NewGuid(),
            WordText = FormWordText.Trim(),
            Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription.Trim(),
            Phonetic_UK = string.IsNullOrWhiteSpace(FormPhoneticUK) ? null : FormPhoneticUK.Trim(),
            Phonetic_US = string.IsNullOrWhiteSpace(FormPhoneticUS) ? null : FormPhoneticUS.Trim(),
            AudioPath_UK = string.IsNullOrWhiteSpace(FormAudioPathUK) ? null : FormAudioPathUK.Trim(),
            AudioPath_US = string.IsNullOrWhiteSpace(FormAudioPathUS) ? null : FormAudioPathUS.Trim(),
            Level = FormLevel,
            WordFamily = familyList
        };

        IsLoading = true;
        try
        {
            if (IsEditing)
            {
                var success = await _vocabService.UpdateVocabularyAsync(dto);
                if (success)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", $"Đã cập nhật từ vựng \"{dto.WordText}\".");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể cập nhật từ vựng.");
                }
            }
            else
            {
                var created = await _vocabService.CreateVocabularyAsync(dto);
                if (created != null)
                {
                    IsFormOpen = false;
                    await LoadDataAsync();
                    await _dialogService.ShowSuccessAsync("Thành công", $"Đã thêm mới từ vựng \"{dto.WordText}\".");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Thất bại", "Không thể thêm mới từ vựng.");
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
    public async Task DeleteAsync(VocabularyDto? item)
    {
        if (item == null) return;

        var confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa từ vựng",
            $"Bạn có chắc chắn muốn xóa từ vựng \"{item.WordText}\"?\nLưu ý: Toàn bộ ngữ nghĩa và ví dụ thuộc từ này cũng sẽ bị xóa vĩnh viễn!");

        if (!confirmed) return;

        IsLoading = true;
        try
        {
            var success = await _vocabService.DeleteVocabularyAsync(item.Id);
            if (success)
            {
                await LoadDataAsync();
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã xóa từ vựng \"{item.WordText}\".");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Thất bại", "Không thể xóa từ vựng đã chọn.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi xóa từ vựng", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
