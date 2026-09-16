using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// Shell ViewModel cho cửa sổ chính (MainWindow) dạng Single Page Application.
/// Quản lý điều phối các Page (thông qua INavigatorService.CurrentPage),
/// các Dialog Modal Overlay (thông qua INavigatorService.DialogStack),
/// và xử lý bảo mật hệ thống / chống gian lận.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // ──────────────────────────── Dependencies ──────────────────────────────

    private readonly INavigatorService _navigator;
    private readonly ISystemControlService _systemControlService;

    /// <summary>
    /// Service điều phối trung chuyển các Page và Dialog trên MainWindow.
    /// Giao diện MainWindow.xaml sẽ trực tiếp bind vào Navigator.CurrentPage và Navigator.DialogStack.
    /// </summary>
    public INavigatorService Navigator => _navigator;

    // ──────────────────────────── Observable Properties ─────────────────────

    [ObservableProperty]
    private bool _isUnlocked;

    /// <summary>
    /// Cho biết hệ thống đã được mở khóa hay chưa (ủy quyền cho QuizViewModel nếu đang hiển thị).
    /// </summary>
    public bool UnlockedState => (_navigator.CurrentPage as QuizViewModel)?.IsUnlocked ?? IsUnlocked;

    // ──────────────────────────── Constructor ───────────────────────────────

    public MainViewModel(
        INavigatorService navigator,
        ISystemControlService systemControlService)
    {
        _navigator = navigator;
        _systemControlService = systemControlService;

        // Khóa Task Manager ngay khi khởi tạo
#if !DEBUG
        _systemControlService.DisableTaskManager();
#endif

        // Điều hướng tới màn hình bài kiểm tra (Quiz UserControl) làm trang chủ mặc định
        _navigator.NavigateTo<LearnWordViewModel>(addToStack: false);

        // Hiển thị màn hình cảnh báo Kiosk-mode trên Dialog overlay
        ShowStartupWarning();
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Tắt ứng dụng khẩn cấp (nút đóng trên giao diện).
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        Application.Current?.Shutdown();
    }

    // ──────────────────────────── Anti-Cheat ────────────────────────────────

    /// <summary>
    /// Được gọi từ MainWindow.OnClosing khi phát hiện user cố tắt cửa sổ mà chưa mở khóa.
    /// </summary>
    public void OnCheatDetected()
    {
        if (_navigator.CurrentPage is QuizViewModel quizVm)
        {
            quizVm.OnCheatDetected();
        }
    }

    // ──────────────────────────── Dialog & Screen Navigation ────────────────

    /// <summary>
    /// Mở màn hình cảnh báo khởi động Kiosk-mode trên MainWindow.
    /// </summary>
    [RelayCommand]
    public void ShowStartupWarning()
    {
        _navigator.OpenDialog<StartupWarningViewModel>();
    }

    /// <summary>
    /// Mở một Dialog bất kỳ lên MainWindow.
    /// </summary>
    public T OpenDialog<T>(params object?[] parameters) where T : class
    {
        return _navigator.OpenDialog<T>(parameters);
    }

    /// <summary>
    /// Mở một Dialog kèm delegate cấu hình.
    /// </summary>
    public T OpenDialog<T>(Action<T>? configure, params object?[] parameters) where T : class
    {
        return _navigator.OpenDialog<T>(configure, parameters);
    }

    /// <summary>
    /// Đóng dialog đang nằm trên đỉnh MainWindow.
    /// </summary>
    [RelayCommand]
    public void CloseTopDialog()
    {
        _navigator.CloseTopDialog();
    }

    /// <summary>
    /// Đóng toàn bộ các dialog đang mở trên MainWindow.
    /// </summary>
    [RelayCommand]
    public void CloseAllDialogs()
    {
        _navigator.CloseAllDialogs();
    }
}
