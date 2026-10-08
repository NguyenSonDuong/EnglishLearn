using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel quản lý chức năng học và luyện gõ từ mới.
/// Binding đầy đủ với VocabularyDto: WordText, Phonetics, WordFamily, Meanings (Synonyms, Antonyms, Examples).
/// Kế thừa PageViewModelBase để đồng bộ điều hướng SPA trong MainWindow.
/// </summary>
public partial class LearnWordViewModel : PageViewModelBase
{
    // ──────────────────────────── Dependencies ──────────────────────────────

    private readonly INavigatorService _navigator;
    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;

    // ──────────────────────────── Internal State ────────────────────────────

    private VocabularyDto? _currentVocabularyDto;

    // ──────────────────────────── Observable Properties (Từ vựng) ───────────

    /// <summary>Từ tiếng Anh gốc cần học.</summary>
    [ObservableProperty]
    private string _wordText = string.Empty;

    /// <summary>Mô tả ý nghĩa chung, bản chất của từ vựng.</summary>
    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>Phiên âm Anh-Anh (UK).</summary>
    [ObservableProperty]
    private string _phoneticUK = string.Empty;

    /// <summary>Phiên âm Anh-Mỹ (US).</summary>
    [ObservableProperty]
    private string _phoneticUS = string.Empty;

    /// <summary>Danh sách các dạng từ họ hàng (WordFamily).</summary>
    [ObservableProperty]
    private ObservableCollection<string> _wordFamily = new();

    /// <summary>Cấp độ CEFR của từ vựng.</summary>
    [ObservableProperty]
    private CEFRLevel _level = CEFRLevel.Uncategorized;


    /// <summary>Danh sách các nghĩa đầy đủ (VocabularyMeaningDto).</summary>
    [ObservableProperty]
    private ObservableCollection<VocabularyMeaningDto> _meanings = new();

    // ──────────────────────────── Observable Properties (Luyện gõ) ──────────

    /// <summary>Nội dung người dùng đang gõ trong ô nhập liệu.</summary>
    [ObservableProperty]
    private string _userInput = string.Empty;

    /// <summary>Lần gõ chính xác hiện tại.</summary>
    [ObservableProperty]
    private int _currentAttempt;

    /// <summary>Tổng số lần cần gõ hoàn thành (mặc định 10).</summary>
    [ObservableProperty]
    private int _targetAttempts = 10;

    /// <summary>Cờ điều khiển việc ẩn/hiện từ gốc ở khu vực luyện gõ.</summary>
    [ObservableProperty]
    private bool _isWordVisible = true;

    /// <summary>Thông báo trạng thái phản hồi cho người dùng.</summary>
    [ObservableProperty]
    private string _feedbackMessage = "💡 Hãy nhìn kỹ từ và gõ lại thật chính xác vào ô bên dưới.";

    /// <summary>Mã màu hex cho thông báo phản hồi.</summary>
    [ObservableProperty]
    private string _feedbackColor = "#6C63FF";

    // ──────────────────────────── Constructor ───────────────────────────────

    public LearnWordViewModel(
        INavigatorService navigator,
        IVocabularyGeneratorService vocabularyGeneratorService)
    {
        _navigator = navigator;
        _vocabularyGeneratorService = vocabularyGeneratorService;
        PageTitle = "Học Từ Mới";
        _ = InitializeVocabularyAsync();
    }


    /// <summary>
    /// Nạp từ vựng khởi đầu từ cơ sở dữ liệu SQLite.
    /// Nếu cơ sở dữ liệu chưa có dữ liệu, tự động fallback về dữ liệu mẫu (mock).
    /// </summary>
    private async Task InitializeVocabularyAsync()
    {
        try
        {
            var dto = await _vocabularyGeneratorService.GenerateVocabularyAsync(CEFRLevel.B2);
            if (dto != null)
            {
                LoadVocabularyDto(dto);
                return;
            }
        }
        catch
        {
            // Bỏ qua lỗi và chuyển sang nạp mock
        }

    }

    // ──────────────────────────── Commands ──────────────────────────────────

    #region RelayCommand

    [RelayCommand]
    private async Task Loaded()
    {
        try
        {
            await LoadedAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Mở Dialog chọn cấp độ CEFR để sinh từ vựng mới.
    /// </summary>
    [RelayCommand]
    private async Task OpenSelectLevel()
    {
        try
        {
            await OnOpenSelectLeverViewAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Xử lý khi người dùng nhấn Enter hoặc nhấn nút Xác nhận.
    /// Để trống theo yêu cầu kiến trúc – sẵn sàng cho logic nghiệp vụ sau.
    /// </summary>
    [RelayCommand]
    private async Task Submit()
    {
        try
        {
            await OnSubmitWordAsync();
        }
        catch(Exception ex)
        {
            throw;
        }
    }

    #endregion
    // ──────────────────────────── Event Handlers ────────────────────────────
    #region Hàm logic nghiệp vụ

    private async Task LoadedAsync()
    {
        try
        {

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Processes a submitted word asynchronously.
    /// </summary>
    /// <remarks>Await the returned task to observe exceptions. Intended for use as an asynchronous event
    /// handler invoked from the UI thread.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task OnSubmitWordAsync()
    {
        try
        {

        }catch(Exception ex)
        {
            throw;
        }
    }

    private async Task OnOpenSelectLeverViewAsync()
    {
        try
        {
            var dialog = _navigator.OpenDialog<SelectLevelViewModel>();
            dialog.VocabularyGenerated += OnVocabularyGenerated;
        }
        catch (Exception ex)
        {
            throw;
        }
    }



    /// <summary>
    /// Được gọi khi SelectLevelViewModel phát ra VocabularyGenerated event.
    /// Cập nhật toàn bộ Observable Properties từ VocabularyDto mới.
    /// </summary>
    private void OnVocabularyGenerated(VocabularyDto dto)
    {
        LoadVocabularyDto(dto);
        CurrentAttempt = 0;
        UserInput = string.Empty;
        FeedbackMessage = $"✨ Đã tải từ mới: \"{dto.WordText}\" — Hãy luyện gõ từ này!";
        FeedbackColor = "#22C55E";
    }

    // ──────────────────────────── Vocabulary Loader ─────────────────────────

    /// <summary>
    /// Map đầy đủ các trường của VocabularyDto vào các Observable Properties.
    /// </summary>
    private void LoadVocabularyDto(VocabularyDto dto)
    {
        _currentVocabularyDto = dto;

        WordText = dto.WordText;
        Description = dto.Description ?? string.Empty;
        PhoneticUK = dto.Phonetic_UK ?? string.Empty;
        PhoneticUS = dto.Phonetic_US ?? string.Empty;
        Level = dto.Level;
        WordFamily = new ObservableCollection<string>(dto.WordFamily);
        Meanings = new ObservableCollection<VocabularyMeaningDto>(dto.Meanings);
    }

    

    #endregion
}
