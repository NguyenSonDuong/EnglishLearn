using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// Model đại diện cho một lựa chọn cấp độ CEFR trong Dialog.
/// </summary>
public class CEFRLevelOption
{
    public CEFRLevel Level { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel cho Dialog chọn cấp độ CEFR và kích hoạt sinh từ vựng.
/// Kế thừa DialogViewModelBase để tích hợp với INavigatorService trên MainWindow.
/// </summary>
public partial class SelectLevelViewModel : DialogViewModelBase
{
    // ──────────────────────────── Dependencies ──────────────────────────────

    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;

    // ──────────────────────────── Observable Properties ─────────────────────

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

    /// <summary>
    /// Sự kiện kích hoạt khi sinh VocabularyDto hợp lệ thành công.
    /// LearnWordViewModel sẽ đăng ký lắng nghe event này.
    /// </summary>
    public event Action<VocabularyDto>? VocabularyGenerated;

    // ──────────────────────────── Constructor ───────────────────────────────

    public SelectLevelViewModel(IVocabularyGeneratorService vocabularyGeneratorService)
    {
        _vocabularyGeneratorService = vocabularyGeneratorService;
        DialogTitle = "Chọn Cấp Độ Từ Vựng";
        InitLevels();
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Sinh từ vựng với cấp độ đã chọn và phát ra VocabularyDto khi thành công.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        if (SelectedLevel is null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var dto = await _vocabularyGeneratorService.GenerateVocabularyAsync(SelectedLevel.Level);

            if (dto is null)
            {
                ErrorMessage = "⚠ Không thể tạo từ vựng. Vui lòng thử lại.";
                return;
            }

            VocabularyGenerated?.Invoke(dto);
            Close();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanConfirm() => SelectedLevel is not null && !IsLoading;

    /// <summary>Người dùng click vào một ô cấp độ → cập nhật SelectedLevel.</summary>
    [RelayCommand]
    private void SelectLevel(CEFRLevelOption option)
    {
        SelectedLevel = option;
    }

    // Khi SelectedLevel thay đổi → cập nhật CanExecute của ConfirmCommand
    partial void OnSelectedLevelChanged(CEFRLevelOption? value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    // Khi IsLoading thay đổi → cập nhật CanExecute của ConfirmCommand
    partial void OnIsLoadingChanged(bool value)
    {
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    // ──────────────────────────── Private Helpers ───────────────────────────

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

        // Mặc định chọn B1
        SelectedLevel = AvailableLevels.FirstOrDefault(x => x.Level == CEFRLevel.B1);
    }
}
