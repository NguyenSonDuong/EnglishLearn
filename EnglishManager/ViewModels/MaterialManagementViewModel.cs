using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

public partial class MaterialManagementViewModel : ObservableObject
{
    private readonly IInAppDialogService _dialogService;
    private readonly IExcelImportService _excelImportService;

    public static readonly List<LearningMaterialDto> MockMaterials = new()
    {
        new LearningMaterialDto
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            DeckId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Term = "Abandon",
            Meaning = "Từ bỏ, ruồng bỏ",
            CategoryType = CategoryType.Vocab,
            Phonetics = "/əˈbændən/",
            ExampleSentence = "He decided to abandon his car and walk in the blizzard.",
            QuestionCount = 1
        },
        new LearningMaterialDto
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            DeckId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Term = "Brilliant",
            Meaning = "Xuất sắc, rực rỡ",
            CategoryType = CategoryType.Vocab,
            Phonetics = "/ˈbrɪljənt/",
            ExampleSentence = "She had a brilliant idea.",
            QuestionCount = 1
        },
        new LearningMaterialDto
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            DeckId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Term = "Coherent",
            Meaning = "Mạch lạc, chặt chẽ",
            CategoryType = CategoryType.Vocab,
            Phonetics = "/koʊˈhɪrənt/",
            ExampleSentence = "They failed to provide a coherent explanation.",
            QuestionCount = 1
        }
    };

    [ObservableProperty]
    private ObservableCollection<DeckDto> _decks = new();

    [ObservableProperty]
    private DeckDto? _filterDeck;

    [ObservableProperty]
    private ObservableCollection<LearningMaterialDto> _allMaterials = new();

    [ObservableProperty]
    private ObservableCollection<LearningMaterialDto> _filteredMaterials = new();

    [ObservableProperty]
    private LearningMaterialDto? _selectedMaterial;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // ── Excel Import State ──
    [ObservableProperty]
    private bool _isImportModalOpen;

    [ObservableProperty]
    private string _importFilePath = string.Empty;

    [ObservableProperty]
    private string _importFileName = string.Empty;

    [ObservableProperty]
    private bool _hasHeaderRow;

    [ObservableProperty]
    private Guid _selectedImportDeckId;

    [ObservableProperty]
    private CategoryType _selectedImportCategoryType = CategoryType.Vocab;

    [ObservableProperty]
    private ObservableCollection<EnglishManager.Models.ExcelImportItem> _importPreviewItems = new();

    [ObservableProperty]
    private int _validImportCount;

    [ObservableProperty]
    private int _totalImportCount;

    [ObservableProperty]
    private bool _isImporting;

    // Danh sách CategoryType cho ComboBox
    public List<CategoryType> AvailableCategories { get; } = Enum.GetValues<CategoryType>().ToList();

    // Form fields
    [ObservableProperty]
    private Guid? _editingMaterialId;

    [ObservableProperty]
    private Guid _formDeckId;

    [ObservableProperty]
    private string _formTerm = string.Empty;

    [ObservableProperty]
    private string _formMeaning = string.Empty;

    [ObservableProperty]
    private CategoryType _formCategoryType = CategoryType.Vocab;

    [ObservableProperty]
    private string _formContextTag = string.Empty;

    [ObservableProperty]
    private string _formPhonetics = string.Empty;

    [ObservableProperty]
    private string _formExampleSentence = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm từ mới / tài liệu";

    public MaterialManagementViewModel(
        IInAppDialogService dialogService,
        IExcelImportService excelImportService)
    {
        _dialogService = dialogService;
        _excelImportService = excelImportService;
    }

    partial void OnHasHeaderRowChanged(bool value)
    {
        if (!string.IsNullOrWhiteSpace(ImportFilePath))
        {
            ParseExcel();
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnFilterDeckChanged(DeckDto? value)
    {
        _ = LoadMaterialsForFilterAsync();
    }

    partial void OnSelectedMaterialChanged(LearningMaterialDto? value)
    {
        if (value != null && !IsEditing)
        {
            LoadIntoForm(value, false);
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

            await LoadMaterialsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải dữ liệu từ vựng: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMaterialsForFilterAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Yield();
            var list = FilterDeck != null
                ? MockMaterials.Where(m => m.DeckId == FilterDeck.Id).ToList()
                : MockMaterials.ToList();

            AllMaterials = new ObservableCollection<LearningMaterialDto>(list.Select(m => new LearningMaterialDto
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
            ApplyFilter();

            if (SelectedMaterial != null)
            {
                SelectedMaterial = AllMaterials.FirstOrDefault(m => m.Id == SelectedMaterial.Id);
            }
            else if (AllMaterials.Count > 0)
            {
                SelectedMaterial = AllMaterials[0];
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách tài liệu: {ex.Message}");
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
            FilteredMaterials = new ObservableCollection<LearningMaterialDto>(AllMaterials);
        }
        else
        {
            var q = SearchText.Trim().ToLowerInvariant();
            var matches = AllMaterials.Where(m =>
                (m.Term != null && m.Term.ToLowerInvariant().Contains(q)) ||
                (m.Meaning != null && m.Meaning.ToLowerInvariant().Contains(q)) ||
                (m.ContextTag != null && m.ContextTag.ToLowerInvariant().Contains(q)) ||
                (m.ExampleSentence != null && m.ExampleSentence.ToLowerInvariant().Contains(q)));
            FilteredMaterials = new ObservableCollection<LearningMaterialDto>(matches);
        }
    }

    [RelayCommand]
    public void StartCreate()
    {
        if (Decks.Count == 0)
        {
            _ = _dialogService.ShowWarningAsync("Chưa có bộ đề", "Bạn cần tạo ít nhất một bộ đề (Deck) trước khi thêm từ mới.");
            return;
        }

        EditingMaterialId = null;
        FormDeckId = FilterDeck?.Id ?? Decks[0].Id;
        FormTerm = string.Empty;
        FormMeaning = string.Empty;
        FormCategoryType = CategoryType.Vocab;
        FormContextTag = string.Empty;
        FormPhonetics = string.Empty;
        FormExampleSentence = string.Empty;
        FormTitle = "Thêm từ mới / tài liệu";
        IsEditing = true;
    }

    [RelayCommand]
    public void StartEdit(LearningMaterialDto? target)
    {
        var item = target ?? SelectedMaterial;
        if (item == null) return;
        LoadIntoForm(item, true);
    }

    private void LoadIntoForm(LearningMaterialDto item, bool enableEdit)
    {
        EditingMaterialId = item.Id;
        FormDeckId = item.DeckId;
        FormTerm = item.Term;
        FormMeaning = item.Meaning;
        FormCategoryType = item.CategoryType;
        FormContextTag = item.ContextTag ?? string.Empty;
        FormPhonetics = item.Phonetics ?? string.Empty;
        FormExampleSentence = item.ExampleSentence ?? string.Empty;
        FormTitle = $"Chỉnh sửa: {item.Term}";
        IsEditing = enableEdit;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FormTerm))
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng nhập từ vựng / thuật ngữ (Term).");
            return;
        }

        if (string.IsNullOrWhiteSpace(FormMeaning))
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng nhập nghĩa hoặc giải thích (Meaning).");
            return;
        }

        if (FormDeckId == Guid.Empty)
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng chọn bộ đề chứa từ vựng này.");
            return;
        }

        try
        {
            IsLoading = true;
            await Task.Yield();
            if (EditingMaterialId == null)
            {
                var dto = new LearningMaterialDto
                {
                    Id = Guid.NewGuid(),
                    DeckId = FormDeckId,
                    Term = FormTerm.Trim(),
                    Meaning = FormMeaning.Trim(),
                    CategoryType = FormCategoryType,
                    ContextTag = string.IsNullOrWhiteSpace(FormContextTag) ? null : FormContextTag.Trim(),
                    Phonetics = string.IsNullOrWhiteSpace(FormPhonetics) ? null : FormPhonetics.Trim(),
                    ExampleSentence = string.IsNullOrWhiteSpace(FormExampleSentence) ? null : FormExampleSentence.Trim(),
                    QuestionCount = 0
                };
                MockMaterials.Add(dto);
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã thêm từ mới '{dto.Term}'.");
            }
            else
            {
                var existing = MockMaterials.FirstOrDefault(m => m.Id == EditingMaterialId.Value);
                if (existing != null)
                {
                    existing.DeckId = FormDeckId;
                    existing.Term = FormTerm.Trim();
                    existing.Meaning = FormMeaning.Trim();
                    existing.CategoryType = FormCategoryType;
                    existing.ContextTag = string.IsNullOrWhiteSpace(FormContextTag) ? null : FormContextTag.Trim();
                    existing.Phonetics = string.IsNullOrWhiteSpace(FormPhonetics) ? null : FormPhonetics.Trim();
                    existing.ExampleSentence = string.IsNullOrWhiteSpace(FormExampleSentence) ? null : FormExampleSentence.Trim();
                }
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã cập nhật từ vựng '{FormTerm.Trim()}'.");
            }

            IsEditing = false;
            await LoadMaterialsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi lưu dữ liệu", $"Thao tác lưu thất bại: {ex.Message}");
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
        if (SelectedMaterial != null)
        {
            LoadIntoForm(SelectedMaterial, false);
        }
    }

    [RelayCommand]
    public async Task DeleteAsync(LearningMaterialDto? target)
    {
        var item = target ?? SelectedMaterial;
        if (item == null) return;

        bool confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa tài liệu",
            $"Bạn có chắc chắn muốn xóa từ vựng '{item.Term}'?\nToàn bộ câu hỏi liên quan đến từ vựng này cũng sẽ bị xóa.");

        if (!confirmed) return;

        try
        {
            IsLoading = true;
            await Task.Yield();
            MockMaterials.RemoveAll(m => m.Id == item.Id);
            await _dialogService.ShowSuccessAsync("Đã xóa", $"Từ vựng '{item.Term}' đã được xóa thành công.");
            await LoadMaterialsForFilterAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi xóa từ vựng", $"Không thể xóa tài liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Excel Import Commands & Logic ──

    [RelayCommand]
    public async Task OpenImportModalAsync()
    {
        if (Decks.Count == 0)
        {
            await _dialogService.ShowWarningAsync(
                "Chưa có bộ đề",
                "Bạn cần tạo ít nhất một bộ đề (Deck) trước khi nhập dữ liệu từ Excel.");
            return;
        }

        SelectedImportDeckId = FilterDeck?.Id ?? Decks[0].Id;
        SelectedImportCategoryType = CategoryType.Vocab;
        ImportFilePath = string.Empty;
        ImportFileName = string.Empty;
        HasHeaderRow = false;
        ImportPreviewItems.Clear();
        ValidImportCount = 0;
        TotalImportCount = 0;
        IsImportModalOpen = true;
    }

    [RelayCommand]
    public void CloseImportModal()
    {
        IsImportModalOpen = false;
        ImportPreviewItems.Clear();
        ImportFilePath = string.Empty;
        ImportFileName = string.Empty;
    }

    [RelayCommand]
    public async Task BrowseExcelFileAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn tệp Excel chứa danh sách từ vựng",
            Filter = "Tệp Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Tất cả tệp (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            ImportFilePath = dialog.FileName;
            ImportFileName = System.IO.Path.GetFileName(dialog.FileName);
            await ParseExcelAsync();
        }
    }

    private async Task ParseExcelAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportFilePath) || !System.IO.File.Exists(ImportFilePath))
        {
            return;
        }

        try
        {
            IsLoading = true;
            var items = await Task.Run(() => _excelImportService.ParseExcelFile(ImportFilePath, HasHeaderRow));
            ImportPreviewItems = new ObservableCollection<EnglishManager.Models.ExcelImportItem>(items);
            TotalImportCount = items.Count;
            ValidImportCount = items.Count(i => i.IsValid);

            if (items.Count == 0)
            {
                await _dialogService.ShowWarningAsync("Tệp trống", "Không tìm thấy dòng dữ liệu nào trong tệp Excel này.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Lỗi đọc tệp Excel",
                $"Không thể đọc tệp Excel '{ImportFileName}':\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ParseExcel()
    {
        _ = ParseExcelAsync();
    }

    [RelayCommand]
    public async Task ExecuteImportAsync()
    {
        if (SelectedImportDeckId == Guid.Empty)
        {
            await _dialogService.ShowWarningAsync("Chưa chọn bộ đề", "Vui lòng chọn bộ đề đích cần nhập từ vựng.");
            return;
        }

        if (ImportPreviewItems.Count == 0)
        {
            await _dialogService.ShowWarningAsync("Chưa có dữ liệu", "Vui lòng chọn tệp Excel hợp lệ trước khi bấm Import.");
            return;
        }

        if (ValidImportCount == 0)
        {
            await _dialogService.ShowWarningAsync("Không có dữ liệu hợp lệ", "Tệp Excel không có dòng từ vựng nào hợp lệ (cần có Từ và Nghĩa).");
            return;
        }

        var targetDeck = Decks.FirstOrDefault(d => d.Id == SelectedImportDeckId);
        string deckName = targetDeck?.Name ?? "bộ đề đã chọn";

        bool confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận nhập dữ liệu",
            $"Bạn có chắc chắn muốn nhập {ValidImportCount} từ vựng vào bộ đề '{deckName}'?");

        if (!confirmed) return;

        try
        {
            IsImporting = true;
            IsLoading = true;

            int importedCount = await _excelImportService.ImportToDatabaseAsync(
                ImportPreviewItems,
                SelectedImportDeckId,
                SelectedImportCategoryType);

            foreach (var preview in ImportPreviewItems.Where(p => p.IsValid))
            {
                MockMaterials.Add(new LearningMaterialDto
                {
                    Id = Guid.NewGuid(),
                    DeckId = SelectedImportDeckId,
                    Term = preview.Term.Trim(),
                    Meaning = preview.Meaning.Trim(),
                    CategoryType = SelectedImportCategoryType,
                    ContextTag = preview.ContextTag,
                    Phonetics = preview.Phonetics,
                    ExampleSentence = preview.ExampleSentence,
                    QuestionCount = 0
                });
            }

            IsImportModalOpen = false;
            await LoadMaterialsForFilterAsync();

            await _dialogService.ShowSuccessAsync(
                "Nhập dữ liệu thành công",
                $"Đã nhập thành công {importedCount} từ vựng mới vào bộ đề '{deckName}'.");

            CloseImportModal();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(
                "Lỗi nhập dữ liệu",
                $"Đã xảy ra lỗi khi lưu từ vựng vào cơ sở dữ liệu:\n{ex.Message}");
        }
        finally
        {
            IsImporting = false;
            IsLoading = false;
        }
    }
}
