using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.DTOs.ViewModelDto;
using English.Entity.Enums;
using English.Entity.Services;
using English.ViewModel.VMException;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel cho Dialog chọn cấp độ CEFR và kích hoạt sinh từ vựng.
/// Kế thừa DialogViewModelBase để tích hợp với INavigatorService trên MainWindow.
/// </summary>
public partial class SelectLevelViewModel : DialogViewModelBase
{

    #region Inject - các service cần thiết
    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;
    #endregion

    #region Binding - danh sách cấp độ / trạng tái tạo từ vựng / Message lỗi
    /// <summary>Danh sách các cấp độ CEFR để hiển thị lên Dialog.</summary>
    [ObservableProperty]
    private List<CEFRLevelOption> _availableLevels = new();

    /// <summary>Cấp độ người dùng đang chọn.</summary>
    [ObservableProperty]
    private CEFRLevelOption? _selectedLevel;

    /// <summary>Trạng thái đang tạo từ vựng (để hiện loading indicator).</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Thông báo lỗi nếu tạo từ vựng thất bại.</summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // ──────────────────────────── Events ────────────────────────────────────
    #endregion

    #region Callback - Hoàn thành lấy từ vựng
    /// <summary>
    /// Sự kiện kích hoạt khi sinh VocabularyDto hợp lệ thành công.
    /// LearnWordViewModel sẽ đăng ký lắng nghe event này.
    /// </summary>
    public event Action<VocabularyDto>? VocabularyGenerated;
    
    #endregion

    public SelectLevelViewModel(IVocabularyGeneratorService vocabularyGeneratorService)
    {
        _vocabularyGeneratorService = vocabularyGeneratorService;
        DialogTitle = "Chọn Cấp Độ Từ Vựng";
        InitLevels();
    }

    #region RelayCommand - Tìm kiếm từ, đóng giao diện ...
    [RelayCommand]
    private async Task Loaded()
    {
        try
        {
            await LoadedAsync();
        }
        catch(Exception)
        {

        }
    }
    
    
    
    /// <summary>
    /// Sinh từ vựng với cấp độ đã chọn và phát ra VocabularyDto khi thành công.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        try
        {
            await SubmitLoadVocabularyAsync();
        }
        catch(Exception)
        {

        }
    }

    private bool CanConfirm() => SelectedLevel is not null && !IsLoading;

    /// <summary>
    /// Người dùng click vào một ô cấp độ → cập nhật SelectedLevel.
    /// </summary>
    /// <param name="option">Cấp độ mà người dùng đã chọn</param>
    [RelayCommand]
    private void SelectLevel(CEFRLevelOption option)
    {
        SelectedLevel = option;
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Callback - Sự kiện khi thay đổi Level và 
    
    // Khi IsLoading thay đổi → cập nhật CanExecute của ConfirmCommand
    partial void OnIsLoadingChanged(bool value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Function Logic - Hàm logic nghiệp vụ - tải dữ liệu / chuyển cấp độ

    // ──────────────────────────── Private Helpers ───────────────────────────

    /// <summary>
    /// Hàm loading ban đầu cho view
    /// </summary>
    /// <returns></returns>
    private async Task LoadedAsync()
    {
        try
        {
            InitLevels();
        }
        catch
        {
            throw;
        }
    }
    private async Task SubmitLoadVocabularyAsync()
    {
        try
        {
            if (SelectedLevel is null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var dto = await _vocabularyGeneratorService.GenerateVocabularyAsync(SelectedLevel.Level);

                if (dto is null)
                {
                    throw new DataException("⚠ Không thể tạo từ vựng.Vui lòng thử lại.");
                }

                VocabularyGenerated?.Invoke(dto);
                Close();
            }
            finally
            {
                IsLoading = false;
            }
        }
        catch
        {
            throw;
        }
    }
    private void InitLevels()
    {
        AvailableLevels = new List<CEFRLevelOption>
        {
            new() { Level = CEFRLevel.A1, DisplayName = "A1", Emoji = "🌱",
                    Description = "Người mới bắt đầu", ColorHex = "#16A34A" },
            new() { Level = CEFRLevel.A2, DisplayName = "A2", Emoji = "🌿",
                    Description = "Cơ bản sơ cấp", ColorHex = "#65A30D" },
            new() { Level = CEFRLevel.B1, DisplayName = "B1", Emoji = "📗",
                    Description = "Trung cấp thấp", ColorHex = "#CA8A04" },
            new() { Level = CEFRLevel.B2, DisplayName = "B2", Emoji = "📘",
                    Description = "Trung cấp cao", ColorHex = "#EA580C" },
            new() { Level = CEFRLevel.C1, DisplayName = "C1", Emoji = "🔥",
                    Description = "Nâng cao", ColorHex = "#DC2626" },
            new() { Level = CEFRLevel.C2, DisplayName = "C2", Emoji = "👑",
                    Description = "Thành thạo hoàn toàn", ColorHex = "#9333EA" },
        };

        // Mặc định chọn A2
        SelectedLevel = AvailableLevels.FirstOrDefault(x => x.Level == CEFRLevel.A2);
    }

    #endregion
}
