using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Application;
using English.ViewModel.ViewModels;
using EnglishLocker.Views;
using Microsoft.Extensions.DependencyInjection;
using WinFormsScreen = System.Windows.Forms.Screen;
using English.Entity.Services;

namespace EnglishLocker.Services;

/// <summary>
/// Service quản lý hiển thị các cửa sổ trên hệ thống đa màn hình (Multi-monitor).
/// 
/// Sử dụng Win32 API SetWindowPos để gán trực tiếp tọa độ vật lý (pixel) vào HWND của Window.
/// Điều này khắc phục triệt để các lỗi thường gặp trong WPF:
///   1. Lỗi WPF WindowState.Maximized tự động nhảy về màn hình chính (Primary Monitor).
///   2. Lỗi tọa độ âm (khi màn hình phụ nằm bên trái màn hình chính, ví dụ: X = -1920).
///   3. Lỗi sai lệch tỉ lệ DPI (DPI scaling) giữa các màn hình khác nhau.
/// </summary>
public class WindowManagerService : IWindowManagerService
{
    // ──────────────────────────── Win32 Imports ─────────────────────────────

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);

    /// <summary>Đặt cửa sổ lên trên cùng của tất cả các cửa sổ thông thường.</summary>
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    /// <summary>Hiển thị cửa sổ.</summary>
    private const uint SWP_SHOWWINDOW = 0x0040;

    /// <summary>Không kích hoạt (focus) cửa sổ này, giữ focus cho cửa sổ hiện tại.</summary>
    private const uint SWP_NOACTIVATE = 0x0010;

    // ──────────────────────────── Fields ────────────────────────────────────

    private readonly IServiceProvider _serviceProvider;
    private MainWindow? _mainWindow;
    private readonly List<BlackoutWindow> _blackoutWindows = new();

    public WindowManagerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // ──────────────────────────── Public Methods ────────────────────────────

    /// <summary>
    /// Định vị và mở màn hình đen (BlackoutWindow) trên tất cả màn hình phụ trước,
    /// sau đó mở MainWindow trên màn hình chính và nhận focus.
    /// </summary>
    public void OpenAllWindows()
    {
        // 1. Quét danh sách tất cả các màn hình kết nối vào máy tính
        var allScreens = WinFormsScreen.AllScreens;
        Debug.WriteLine($"[WindowManager] Số màn hình phát hiện: {allScreens.Length}");

        // 2. Tìm màn hình chính
        var primaryScreen = allScreens.FirstOrDefault(s => s.Primary) 
                            ?? WinFormsScreen.PrimaryScreen 
                            ?? allScreens[0];
#if !DEBUG
        // 3. MỞ CÁC BLACKOUT WINDOW TRÊN MÀN HÌNH PHỤ TRƯỚC
        foreach (var screen in allScreens)
        {
            // Bỏ qua màn hình chính (chỉ xử lý màn hình phụ)
            if (screen.Primary)
                continue;

            Debug.WriteLine($"[WindowManager] Đặt BlackoutWindow lên màn hình phụ: {screen.DeviceName} tại [{screen.Bounds.X}, {screen.Bounds.Y}, {screen.Bounds.Width}x{screen.Bounds.Height}]");

            var blackout = new BlackoutWindow();
            SetupAndShowWindow(blackout, screen, isPrimary: false);
            _blackoutWindows.Add(blackout);
        }
#endif

        // 4. MỞ MAINWINDOW TRÊN MÀN HÌNH CHÍNH
        Debug.WriteLine($"[WindowManager] Đặt MainWindow lên màn hình chính: {primaryScreen.DeviceName} tại [{primaryScreen.Bounds.X}, {primaryScreen.Bounds.Y}, {primaryScreen.Bounds.Width}x{primaryScreen.Bounds.Height}]");

        _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        Application.Current.MainWindow = _mainWindow;
        SetupAndShowWindow(_mainWindow, primaryScreen, isPrimary: true);

        // Đảm bảo MainWindow luôn nằm trên cùng và nhận bàn phím
        _mainWindow.Activate();
        _mainWindow.Focus();
    }

    /// <summary>
    /// Mở khóa và giải phóng toàn bộ màn hình khi hoàn thành bài thi.
    /// </summary>
    public void UnlockAllScreens()
    {
        void PerformUnlock()
        {
            // Đóng tất cả BlackoutWindow
            foreach (var blackout in _blackoutWindows)
            {
                blackout.CanClose = true;
                blackout.Close();
            }
            _blackoutWindows.Clear();

            // Cho phép đóng MainWindow và đóng cửa sổ
            if (_mainWindow != null)
            {
                if (_mainWindow.DataContext is MainViewModel vm)
                {
                    vm.IsUnlocked = true;
                }
                _mainWindow.Close();
            }
        }

        if (Application.Current.Dispatcher.CheckAccess())
        {
            PerformUnlock();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(PerformUnlock);
        }
    }

    // ──────────────────────────── Private Helpers ───────────────────────────

    /// <summary>
    /// Định vị Window vào đúng màn hình vật lý chỉ định và hiển thị.
    /// Sử dụng Win32 SetWindowPos thay vì WPF Maximized để tránh lỗi tự nhảy về màn hình chính.
    /// </summary>
    private static void SetupAndShowWindow(Window window, WinFormsScreen screen, bool isPrimary)
    {
        // 1. Đặt cấu hình WPF cơ bản (giữ WindowState = Normal, KHÔNG dùng Maximized)
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.WindowState = WindowState.Normal;
        window.Left = screen.Bounds.Left;
        window.Top = screen.Bounds.Top;
        window.Width = screen.Bounds.Width;
        window.Height = screen.Bounds.Height;

        uint flags = SWP_SHOWWINDOW;
        if (!isPrimary)
        {
            // Màn hình phụ: không cướp focus của người dùng
            flags |= SWP_NOACTIVATE;
        }

        // 2. Gán vị trí bằng Win32 API ngay khi HWND được tạo (trước khi vẽ lên màn hình)
        window.SourceInitialized += (sender, args) =>
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                SetWindowPos(
                    hwnd,
                    HWND_TOPMOST,
                    screen.Bounds.X,
                    screen.Bounds.Y,
                    screen.Bounds.Width,
                    screen.Bounds.Height,
                    flags);
            }
        };

        // 3. Hiển thị cửa sổ
        window.Show();

        // 4. Gọi lại SetWindowPos lần nữa sau khi Show để đảm bảo kích thước bao trùm tuyệt đối
        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero)
        {
            SetWindowPos(
                handle,
                HWND_TOPMOST,
                screen.Bounds.X,
                screen.Bounds.Y,
                screen.Bounds.Width,
                screen.Bounds.Height,
                flags);
        }
    }
}
