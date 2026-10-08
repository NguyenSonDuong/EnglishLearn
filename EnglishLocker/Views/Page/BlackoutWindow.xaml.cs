using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace EnglishLocker.Views;

/// <summary>
/// Code-behind cho BlackoutWindow (màn hình đen che màn hình phụ).
/// 
/// Nhiệm vụ:
///   - Phủ kín màn hình phụ được chỉ định.
///   - Chặn đóng cửa sổ (ví dụ: Alt+F4) bằng cờ CanClose.
///   - Nếu người dùng click vào màn hình này, tự động chuyển focus về MainWindow.
/// </summary>
public partial class BlackoutWindow : Window
{
    /// <summary>
    /// Cờ cho phép đóng cửa sổ. Mặc định là false để chặn Alt+F4.
    /// Chỉ được bật thành true thông qua WindowManagerService khi đã mở khóa hợp lệ.
    /// </summary>
    public bool CanClose { get; set; } = false;

    public BlackoutWindow()
    {
        InitializeComponent();

        // Nếu người dùng click vào màn hình phụ, chuyển tiêu điểm về cửa sổ câu hỏi chính
        MouseDown += OnWindowMouseDown;
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
    {
        var mainWin = Application.Current?.MainWindow;
        if (mainWin != null && mainWin.IsVisible)
        {
            mainWin.Activate();
            mainWin.Focus();
        }
    }

    /// <summary>
    /// Ghi đè sự kiện OnClosing để ngăn chặn người dùng tắt cửa sổ này khi chưa mở khóa.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!CanClose)
        {
            // Chặn Alt + F4 hoặc bất kỳ nỗ lực đóng cửa sổ nào
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }
}
