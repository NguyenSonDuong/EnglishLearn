using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel cho màn hình cảnh báo người dùng trước khi kích hoạt Kiosk Mode khóa toàn bộ hệ thống.
/// Kế thừa DialogViewModelBase để tự động tích hợp với INavigatorService trên MainWindow.
/// </summary>
public partial class StartupWarningViewModel : DialogViewModelBase
{
    // ──────────────────────────── Observable Properties ─────────────────────

    [ObservableProperty]
    private string _title = "CẢNH BÁO QUAN TRỌNG VỀ CHẾ ĐỘ KHÓA";

    [ObservableProperty]
    private string _subtitle = "Vui lòng đọc kỹ thông tin dưới đây trước khi quyết định tiếp tục phiên học:";

    /// <summary>
    /// Cho biết ứng dụng có đang chạy với tư cách Windows Shell khởi động máy hay không.
    /// </summary>
    [ObservableProperty]
    private bool _isRunningAsShell;

    public StartupWarningViewModel(IShellManagementService? shellService = null)
    {
        _isRunningAsShell = shellService?.IsRunningAsShell ?? false;

        if (_isRunningAsShell)
        {
            Title = "🔒 CẢNH BÁO: CHẾ ĐỘ KHÓA SHELL KHỞI ĐỘNG MÁY";
            Subtitle = "Ứng dụng được kích hoạt tự động với vai trò Windows Shell khi đăng nhập máy tính:";
        }
        else
        {
            Title = "CẢNH BÁO QUAN TRỌNG VỀ CHẾ ĐỘ KHÓA";
            Subtitle = "Ứng dụng đang mở ở chế độ thông thường (Thử nghiệm / Không phải Shell khởi động máy):";
        }
    }

    [ObservableProperty]
    private string _lockWarningText =
        "Khi chọn 'ĐỒNG Ý', ứng dụng sẽ chuyển sang chế độ Kiosk Mode toàn màn hình và khóa toàn bộ thao tác hệ thống:\n\n" +
        "• Chặn hoàn toàn các phím tắt hệ thống: Alt+F4, Alt+Tab, phím Windows, Task Manager, Ctrl+Shift+Esc.\n" +
        "• Che đen và vô hiệu hóa tương tác trên toàn bộ màn hình phụ vật lý.\n" +
        "• Ngay cả khi khởi động lại máy tính, hệ thống vẫn sẽ bị khóa cho tới khi bạn hoàn thành bài kiểm tra.";

    [ObservableProperty]
    private string _adviceText =
        "Nếu bạn đang có việc bận, đang làm việc dở dang hoặc không có thời gian thật sự rảnh để tập trung học tập, vui lòng nhấn 'TỪ CHỐI' để tắt ứng dụng ngay bây giờ.";

    // ──────────────────────────── Events & Callbacks ────────────────────────

    /// <summary>Sự kiện kích hoạt khi người dùng nhấn Đồng ý tiếp tục.</summary>
    public event Action? Accepted;

    /// <summary>Sự kiện kích hoạt khi người dùng nhấn Từ chối.</summary>
    public event Action? Rejected;

    // ──────────────────────────── Relay Commands ────────────────────────────

    /// <summary>
    /// Khung RelayCommand xử lý khi người dùng nhấn "Đồng ý" để bắt đầu học:
    /// Kích hoạt sự kiện Accepted và tự đóng Dialog khỏi Navigator DialogStack trên MainWindow.
    /// </summary>
    [RelayCommand]
    public void Accept()
    {
        Accepted?.Invoke();
        Close();
    }

    /// <summary>
    /// Khung RelayCommand xử lý khi người dùng nhấn "Từ chối":
    /// Tắt ứng dụng (Shutdown) ngay lập tức để người dùng quay lại làm việc.
    /// </summary>
    [RelayCommand]
    public void Reject()
    {
        Rejected?.Invoke();
        Application.Current?.Shutdown();
    }
}
