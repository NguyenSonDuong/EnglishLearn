using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel chính – quản lý luồng câu hỏi, logic chống gian lận và mở khóa hệ thống.
/// Kế thừa ObservableObject (CommunityToolkit.Mvvm) để hỗ trợ data binding.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // ──────────────────────────── Dependencies ──────────────────────────────

    private readonly IQuestionService _questionService;
    private readonly ISystemControlService _systemControlService;
    private readonly IWindowManagerService _windowManagerService;
    private readonly IShellManagementService _shellManagementService;

    // ──────────────────────────── Internal State ────────────────────────────

    private List<QuestionDto> _questions = new();
    private int _currentIndex;
    private int _cheatAttempts;

    // ──────────────────────────── Observable Properties ─────────────────────

    /// <summary>Câu hỏi đang hiển thị.</summary>
    [ObservableProperty]
    private QuestionDto? _currentQuestion;

    /// <summary>Đáp án người dùng đang chọn (từ RadioButton).</summary>
    [ObservableProperty]
    private string? _selectedAnswer;

    /// <summary>Thông báo kết quả (đúng/sai/gian lận).</summary>
    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    /// <summary>
    /// Cờ cho phép đóng cửa sổ hợp lệ.
    /// false khi đóng → phát hiện gian lận → shutdown.
    /// </summary>
    [ObservableProperty]
    private bool _isUnlocked;

    /// <summary>Số câu trả lời đúng / tổng số câu (tiến trình).</summary>
    [ObservableProperty]
    private string _progressText = string.Empty;

    /// <summary>Số lần phát hiện gian lận.</summary>
    [ObservableProperty]
    private int _cheatCount;

    // ──────────────────────────── Constructor ───────────────────────────────

    public MainViewModel(
        IQuestionService questionService,
        ISystemControlService systemControlService,
        IWindowManagerService windowManagerService,
        IShellManagementService shellManagementService)
    {
        _questionService = questionService;
        _systemControlService = systemControlService;
        _windowManagerService = windowManagerService;
        _shellManagementService = shellManagementService;

        _currentIndex = 0;
        _cheatAttempts = 0;

        // Khóa Task Manager ngay khi khởi tạo
        _systemControlService.DisableTaskManager();

        // Tải câu hỏi bất đồng bộ
        _ = LoadQuestionsAsync();
    }

    // ──────────────────────────── Async Init ─────────────────────────────────

    /// <summary>Tải danh sách câu hỏi từ database.</summary>
    private async Task LoadQuestionsAsync()
    {
        try
        {
            _questions = await _questionService.GetRandomQuestionsAsync(10);
            _currentIndex = 0;
            LoadCurrentQuestion();
        }
        catch
        {
            FeedbackMessage = "⚠ Không thể tải câu hỏi từ cơ sở dữ liệu.";
        }
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Khi user click vào một đáp án (RadioButton), lưu đáp án được chọn.
    /// </summary>
    [RelayCommand]
    private void SelectAnswer(string? answer)
    {
        SelectedAnswer = answer;
    }

    /// <summary>
    /// Kiểm tra đáp án. Nếu đúng → chuyển câu hoặc mở khóa.
    /// </summary>
    [RelayCommand]
    private void SubmitAnswer()
    {
        if (CurrentQuestion is null || string.IsNullOrEmpty(SelectedAnswer))
        {
            FeedbackMessage = "⚠ Vui lòng chọn một đáp án!";
            return;
        }

        if (SelectedAnswer == CurrentQuestion.CorrectAnswer)
        {
            // ── ĐÚNG ──
            _currentIndex++;

            if (_currentIndex >= _questions.Count)
            {
                // Đã trả lời hết tất cả câu hỏi → MỞ KHÓA TOÀN BỘ MÀN HÌNH
                FeedbackMessage = "🎉 Chúc mừng! Bạn đã trả lời đúng tất cả. Màn hình sẽ được mở khóa.";
                UnlockScreen();
            }
            else
            {
                FeedbackMessage = "✅ Chính xác! Chuyển sang câu tiếp theo...";
                LoadCurrentQuestion();
            }
        }
        else
        {
            // ── SAI → giữ nguyên câu, yêu cầu thử lại ──
            FeedbackMessage = "❌ Sai rồi! Hãy thử lại.";
        }
    }

    // ──────────────────────────── Anti-Cheat ────────────────────────────────

    /// <summary>
    /// Được gọi từ code-behind (MainWindow.OnClosing) khi phát hiện
    /// user cố đóng cửa sổ mà IsUnlocked = false.
    /// </summary>
    public void OnCheatDetected()
    {
        _cheatAttempts++;
        CheatCount = _cheatAttempts;

        if (_cheatAttempts >= 3)
        {
            // Quá 3 lần gian lận → tắt máy
            FeedbackMessage = $"🚨 Phát hiện gian lận {_cheatAttempts} lần! Hệ thống sẽ tắt máy...";
            _systemControlService.ShutdownComputer();
        }
        else
        {
            FeedbackMessage = $"🚨 Cảnh báo gian lận! (Lần {_cheatAttempts}/3) – Trả lời đúng để mở khóa.";
        }
    }

    // ──────────────────────────── Private Helpers ───────────────────────────

    /// <summary>Load câu hỏi tại vị trí _currentIndex lên UI.</summary>
    private void LoadCurrentQuestion()
    {
        if (_currentIndex < _questions.Count)
        {
            CurrentQuestion = _questions[_currentIndex];
            SelectedAnswer = null;
            ProgressText = $"Câu {_currentIndex + 1} / {_questions.Count}";
        }
    }

    /// <summary>
    /// Mở khóa màn hình:
    ///   1. Bật lại Task Manager
    ///   2. Đánh dấu IsUnlocked = true (cho phép đóng cửa sổ)
    ///   3. Giải phóng toàn bộ màn hình và khởi động lại Windows Explorer
    /// </summary>
    private async void UnlockScreen()
    {
        _systemControlService.EnableTaskManager();
        IsUnlocked = true;

        // Delay 1.5s để user đọc thông báo chúc mừng, sau đó giải phóng toàn bộ màn hình và nạp lại Windows Shell
        await Task.Delay(1500);
        _windowManagerService.UnlockAllScreens();
        await _shellManagementService.StartExplorerAndExitAsync();
    }
}
