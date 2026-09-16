using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

public partial class DeckManagementViewModel : ObservableObject
{
    private readonly IInAppDialogService _dialogService;

    public static readonly List<DeckDto> MockDecks = new()
    {
        new DeckDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Oxford 3000 Core",
            Description = "Bộ từ vựng tiếng Anh giao tiếp cốt lõi Oxford 3000",
            IsActive = true,
            MaterialCount = 2
        },
        new DeckDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "IELTS Academic",
            Description = "Từ vựng học thuật chuyên sâu cho kỳ thi IELTS",
            IsActive = true,
            MaterialCount = 1
        }
    };

    [ObservableProperty]
    private ObservableCollection<DeckDto> _decks = new();

    [ObservableProperty]
    private ObservableCollection<DeckDto> _filteredDecks = new();

    [ObservableProperty]
    private DeckDto? _selectedDeck;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // Form fields
    [ObservableProperty]
    private Guid? _editingDeckId;

    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private bool _formIsActive = true;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _formTitle = "Thêm bộ đề mới";

    public DeckManagementViewModel(IInAppDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedDeckChanged(DeckDto? value)
    {
        if (value != null && !IsEditing)
        {
            LoadIntoForm(value, false);
        }
    }

    [RelayCommand]
    public async Task LoadDecksAsync()
    {
        try
        {
            IsLoading = true;
            await Task.Yield();
            Decks = new ObservableCollection<DeckDto>(MockDecks.Select(d => new DeckDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                MaterialCount = d.MaterialCount
            }));
            ApplyFilter();

            if (SelectedDeck != null)
            {
                SelectedDeck = Decks.FirstOrDefault(d => d.Id == SelectedDeck.Id);
            }
            else if (Decks.Count > 0)
            {
                SelectedDeck = Decks[0];
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi tải dữ liệu", $"Không thể tải danh sách bộ đề: {ex.Message}");
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
            FilteredDecks = new ObservableCollection<DeckDto>(Decks);
        }
        else
        {
            var query = SearchText.Trim().ToLowerInvariant();
            var matches = Decks.Where(d =>
                (d.Name != null && d.Name.ToLowerInvariant().Contains(query)) ||
                (d.Description != null && d.Description.ToLowerInvariant().Contains(query)));
            FilteredDecks = new ObservableCollection<DeckDto>(matches);
        }
    }

    [RelayCommand]
    public void StartCreate()
    {
        EditingDeckId = null;
        FormName = string.Empty;
        FormDescription = string.Empty;
        FormIsActive = true;
        FormTitle = "Thêm bộ đề mới";
        IsEditing = true;
    }

    [RelayCommand]
    public void StartEdit(DeckDto? target)
    {
        var item = target ?? SelectedDeck;
        if (item == null) return;
        LoadIntoForm(item, true);
    }

    private void LoadIntoForm(DeckDto item, bool enableEdit)
    {
        EditingDeckId = item.Id;
        FormName = item.Name;
        FormDescription = item.Description ?? string.Empty;
        FormIsActive = item.IsActive;
        FormTitle = $"Chỉnh sửa: {item.Name}";
        IsEditing = enableEdit;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FormName))
        {
            await _dialogService.ShowWarningAsync("Thiếu thông tin", "Vui lòng nhập tên bộ đề.");
            return;
        }

        try
        {
            IsLoading = true;
            await Task.Yield();
            if (EditingDeckId == null)
            {
                // Thêm mới
                var dto = new DeckDto
                {
                    Id = Guid.NewGuid(),
                    Name = FormName.Trim(),
                    Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription.Trim(),
                    IsActive = FormIsActive,
                    MaterialCount = 0
                };
                MockDecks.Add(dto);
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã tạo mới bộ đề '{dto.Name}'.");
            }
            else
            {
                // Cập nhật
                var existing = MockDecks.FirstOrDefault(d => d.Id == EditingDeckId.Value);
                if (existing != null)
                {
                    existing.Name = FormName.Trim();
                    existing.Description = string.IsNullOrWhiteSpace(FormDescription) ? null : FormDescription.Trim();
                    existing.IsActive = FormIsActive;
                }
                await _dialogService.ShowSuccessAsync("Thành công", $"Đã cập nhật bộ đề '{FormName.Trim()}'.");
            }

            IsEditing = false;
            await LoadDecksAsync();
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
        if (SelectedDeck != null)
        {
            LoadIntoForm(SelectedDeck, false);
        }
    }

    [RelayCommand]
    public async Task DeleteAsync(DeckDto? target)
    {
        var item = target ?? SelectedDeck;
        if (item == null) return;

        bool confirmed = await _dialogService.ShowConfirmAsync(
            "Xác nhận xóa bộ đề",
            $"Bạn có chắc chắn muốn xóa bộ đề '{item.Name}'?\nLưu ý: Toàn bộ từ vựng và câu hỏi thuộc bộ đề này cũng sẽ bị xóa vĩnh viễn.");

        if (!confirmed) return;

        try
        {
            IsLoading = true;
            await Task.Yield();
            MockDecks.RemoveAll(d => d.Id == item.Id);
            await _dialogService.ShowSuccessAsync("Đã xóa", $"Bộ đề '{item.Name}' đã được xóa thành công.");
            await LoadDecksAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi xóa bộ đề", $"Không thể xóa bộ đề: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ToggleStatusAsync(DeckDto? item)
    {
        if (item == null) return;

        try
        {
            await Task.Yield();
            var target = MockDecks.FirstOrDefault(d => d.Id == item.Id);
            if (target != null)
            {
                target.IsActive = !target.IsActive;
            }
            await LoadDecksAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Lỗi cập nhật", $"Không thể đổi trạng thái: {ex.Message}");
        }
    }
}
