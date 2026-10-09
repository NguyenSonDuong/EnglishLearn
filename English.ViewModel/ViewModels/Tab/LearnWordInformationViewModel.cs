using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel quản lý phần hiển thị thông tin chi tiết từ vựng (Information Panel).
/// </summary>
public partial class LearnWordInformationViewModel : ObservableObject
{
    // ──────────────────────────── Dependencies ──────────────────────────────
    private readonly INavigatorService _navigator;
    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;

    // ──────────────────────────── Internal State ────────────────────────────
    private VocabularyDto? _currentVocabularyDto;

    // ──────────────────────────── Observable Properties ─────────────────────

    /// <summary>Từ tiếng Anh gốc cần học.</summary>
    [ObservableProperty]
    private string _wordText = string.Empty;

    /// <summary>Mô tả ý nghĩa chung của từ vựng.</summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>Phiên âm Anh-Anh (UK).</summary>
    [ObservableProperty]
    private string _phoneticUK = string.Empty;

    /// <summary>Phiên âm Anh-Mỹ (US).</summary>
    [ObservableProperty]
    private string _phoneticUS = string.Empty;

    /// <summary>Danh sách họ hàng từ (WordFamily).</summary>
    [ObservableProperty]
    private ObservableCollection<string> _wordFamily = new();

    /// <summary>Cấp độ CEFR của từ vựng.</summary>
    [ObservableProperty]
    private CEFRLevel _level = CEFRLevel.Uncategorized;

    /// <summary>Danh sách các nghĩa chi tiết của từ (VocabularyMeaningDto).</summary>
    [ObservableProperty]
    private ObservableCollection<VocabularyMeaningDto> _meanings = new();

    /// <summary>Cờ hiển thị chi tiết từ (true: hiển thị nội dung, false: hiển thị placeholder bài học).</summary>
    [ObservableProperty]
    private bool _isWordVisible = true;

    // ──────────────────────────── Events ────────────────────────────────────

    /// <summary>Sự kiện phát ra khi từ vựng được thay đổi hoặc nạp mới thành công.</summary>
    public event Action<VocabularyDto>? VocabularyChanged;

    // ──────────────────────────── Constructor ───────────────────────────────

    public LearnWordInformationViewModel(
        INavigatorService navigator,
        IVocabularyGeneratorService vocabularyGeneratorService)
    {
        _navigator = navigator;
        _vocabularyGeneratorService = vocabularyGeneratorService;
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Đổi sang một từ mới cùng cấp độ hiện tại.
    /// </summary>
    [RelayCommand]
    private async Task ReloadVocabulary()
    {
        try
        {
            await InitializeVocabularyAsync(Level);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Mở Dialog cho phép người dùng chọn cấp độ CEFR.
    /// </summary>
    [RelayCommand]
    private async Task OpenSelectLevel()
    {
        try
        {
            var dialog = _navigator.OpenDialog<SelectLevelViewModel>();
            dialog.VocabularyGenerated += OnVocabularyGenerated;
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            throw;
        }
    }

    // ──────────────────────────── Public & Private Methods ──────────────────

    /// <summary>
    /// Nạp từ vựng mới theo cấp độ chỉ định.
    /// </summary>
    public async Task InitializeVocabularyAsync(CEFRLevel? level = null)
    {
        try
        {
            var targetLevel = level ?? Level;
            var dto = await _vocabularyGeneratorService.GenerateVocabularyAsync(targetLevel);
            if (dto != null)
            {
                LoadVocabularyDto(dto);
                VocabularyChanged?.Invoke(dto);
            }
        }
        catch (Exception)
        {
            // Bỏ qua lỗi và giữ nguyên trạng thái nếu có sự cố
        }
    }

    /// <summary>
    /// Gán thông tin từ vựng từ DTO vào các Observable Properties.
    /// </summary>
    public void LoadVocabularyDto(VocabularyDto dto)
    {
        _currentVocabularyDto = dto;

        WordText = dto.WordText;
        Description = dto.Description ?? string.Empty;
        PhoneticUK = dto.Phonetic_UK ?? string.Empty;
        PhoneticUS = dto.Phonetic_US ?? string.Empty;
        Level = dto.Level;
        WordFamily = new ObservableCollection<string>(dto.WordFamily);
        Meanings = new ObservableCollection<VocabularyMeaningDto>(dto.Meanings);
        IsWordVisible = true;
    }

    private void OnVocabularyGenerated(VocabularyDto dto)
    {
        LoadVocabularyDto(dto);
        VocabularyChanged?.Invoke(dto);
    }
}
