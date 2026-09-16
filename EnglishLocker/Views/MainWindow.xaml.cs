using System.ComponentModel;
using System.Windows;
using English.Entity.Services;
using English.ViewModel.ViewModels;

namespace EnglishLocker.Views;

/// <summary>
/// Code-behind cho MainWindow.
/// 
/// Nhiệm vụ:
///   1. Khởi tạo và hủy KeyboardHookService khi Window mở/đóng.
///   2. Ghi đè OnClosing để chặn đóng cửa sổ khi chưa mở khóa.
///   3. Gọi ViewModel.OnCheatDetected() khi phát hiện gian lận.
/// 
/// Lưu ý: Đây là phần code-behind hợp lệ trong MVVM – chỉ xử lý
/// các tương tác liên quan đến Window mà không thể binding thuần túy.
/// </summary>
public partial class MainWindow : Window
{
    private readonly IHookService _hookService;

    public MainWindow(MainViewModel viewModel, IHookService hookService)
    {
        InitializeComponent();

        // Gán ViewModel làm DataContext cho data binding
        DataContext = viewModel;

        // Lưu reference để quản lý lifecycle
        _hookService = hookService;

        // ── Sự kiện Window ──
        Loaded += OnWindowLoaded;
        Closing += OnWindowClosing;
        Closed += OnWindowClosed;
    }

    // ──────────────────────────── Event Handlers ────────────────────────────

    /// <summary>
    /// Khi Window load xong → cài đặt keyboard hook để chặn Alt+Tab, Win key, ...
    /// </summary>
    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        _hookService.InstallHook();

        // Force focus vào cửa sổ để đảm bảo nó ở trên cùng
        Activate();
        Focus();
    }

    /// <summary>
    /// Ghi đè sự kiện Closing:
    ///   - Nếu ViewModel cho phép (IsUnlocked = true) → đóng bình thường.
    ///   - Nếu chưa cho phép → CHẶN đóng (e.Cancel = true) + ghi nhận gian lận.
    /// </summary>
    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
#if !DEBUG
        if (DataContext is MainViewModel vm && !vm.IsUnlocked)
        {
            // ⛔ CHẶN ĐÓNG CỬA SỔ
            e.Cancel = true;

            // Ghi nhận hành vi gian lận
            vm.OnCheatDetected();

            // Đảm bảo cửa sổ vẫn ở trên cùng
            Topmost = true;
            Activate();
        }
#endif
    }

    /// <summary>
    /// Khi Window đóng thành công (chỉ xảy ra khi IsUnlocked = true)
    /// → gỡ bỏ keyboard hook, giải phóng tài nguyên.
    /// </summary>
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _hookService.UninstallHook();
        _hookService.Dispose();
    }
}
