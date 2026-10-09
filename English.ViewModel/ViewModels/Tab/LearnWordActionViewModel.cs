using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel quản lý phần hành động luyện gõ từ mới (Action Panel).
/// </summary>
public partial class LearnWordActionViewModel : ObservableObject, IDisposable
{
    // ──────────────────────────── Observable Properties ─────────────────────

    /// <summary>Từ tiếng Anh mục tiêu cần gõ.</summary>
    [ObservableProperty]
    private string _wordText = string.Empty;

    /// <summary>Nội dung người dùng đang gõ trong ô nhập liệu.</summary>
    [ObservableProperty]
    private string _userInput = string.Empty;

    /// <summary>Số lần gõ chính xác hiện tại.</summary>
    [ObservableProperty]
    private int _currentAttempt;

    /// <summary>Tổng số lần cần gõ hoàn thành (mặc định 10).</summary>
    [ObservableProperty]
    private int _targetAttempts = 10;

    /// <summary>Cờ ẩn/hiện từ mẫu trong khu vực luyện gõ.</summary>
    [ObservableProperty]
    private bool _isWordVisible = true;

    /// <summary>Thông báo trạng thái phản hồi cho người dùng.</summary>
    [ObservableProperty]
    private string _feedbackMessage = "💡 Hãy nhìn kỹ từ và gõ lại thật chính xác vào ô bên dưới.";

    /// <summary>Mã màu hex cho thông báo phản hồi.</summary>
    [ObservableProperty]
    private string _feedbackColor = "#6C63FF";

    // ──────────────────────────── Events ────────────────────────────────────

    /// <summary>Sự kiện thông báo thay đổi trạng thái ẩn/hiện từ (ví dụ ẩn sau 3 lần gõ đúng).</summary>
    public event Action<bool>? WordVisibilityChanged;

    /// <summary>Sự kiện thông báo khi người dùng đã hoàn thành đủ số lượt gõ yêu cầu cho từ hiện tại.</summary>
    public event Func<Task>? WordCompleted;

    // ──────────────────────────── Constructor ───────────────────────────────

    public LearnWordActionViewModel()
    {
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Xử lý khi người dùng nhấn Enter hoặc nút Xác nhận.
    /// </summary>
    [RelayCommand]
    private async Task Submit()
    {
        try
        {
            await OnSubmitWordAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    // ──────────────────────────── Business Logic ────────────────────────────

    private async Task OnSubmitWordAsync()
    {
        if (UserInput == WordText)
        {
            CurrentAttempt++;
            if (CurrentAttempt > 3)
            {
                IsWordVisible = false;
                WordVisibilityChanged?.Invoke(false);
                FeedbackMessage = "🧠 Hãy nhớ lại từ và tiếp tục gõ!";
                FeedbackColor = "#FACC15";
            }
            else
            {
                FeedbackMessage = "✅ Chính xác! Tiếp tục gõ nào.";
                FeedbackColor = "#22C55E";
            }

            if (CurrentAttempt >= TargetAttempts)
            {
                CurrentAttempt = 0;
                TargetAttempts = 10;
                IsWordVisible = true;
                WordVisibilityChanged?.Invoke(true);
                FeedbackMessage = "🎉 Xuất sắc! Bạn đã hoàn thành từ vựng này.";
                FeedbackColor = "#22C55E";

                if (WordCompleted != null)
                {
                    await WordCompleted.Invoke();
                }
            }
        }
        else
        {
            TargetAttempts += 2;
            CurrentAttempt = Math.Max(0, CurrentAttempt - 3);
            FeedbackMessage = "❌ Chưa chính xác, hãy thử lại!";
            FeedbackColor = "#EF4444";
        }

        UserInput = string.Empty;
    }

    /// <summary>
    /// Thiết lập từ mục tiêu mới và làm mới tiến trình luyện gõ.
    /// </summary>
    public void SetupWord(string wordText, string? feedbackMessage = null)
    {
        WordText = wordText;
        CurrentAttempt = 0;
        TargetAttempts = 10;
        IsWordVisible = true;
        UserInput = string.Empty;
        FeedbackMessage = feedbackMessage ?? $"✨ Đã tải từ mới: \"{wordText}\" — Hãy luyện gõ từ này!";
        FeedbackColor = "#22C55E";
        WordVisibilityChanged?.Invoke(true);
    }

    /// <summary>
    /// Giải phóng toàn bộ event subscription và xóa sạch state để tab cũ không còn tồn tại.
    /// </summary>
    public void Dispose()
    {
        WordVisibilityChanged = null;
        WordCompleted = null;
        UserInput = string.Empty;
        WordText = string.Empty;
        FeedbackMessage = string.Empty;
        CurrentAttempt = 0;
    }
}
