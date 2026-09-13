namespace English.Entity.Services;

/// <summary>
/// Service quản lý cấu hình Custom Shell trên Windows:
///   - Thay thế Windows Explorer bằng ứng dụng EnglishLocker khi đăng nhập.
///   - Khởi động lại Windows Explorer (Desktop, Taskbar) khi hoàn thành bài tập.
///   - Khôi phục cấu hình Shell mặc định khi cần gỡ bỏ.
/// </summary>
public interface IShellManagementService
{
    /// <summary>
    /// Tham số dòng lệnh dùng để nhận diện ứng dụng được kích hoạt bởi Windows Shell khi khởi động máy.
    /// </summary>
    public const string KioskShellArgument = "--kiosk-shell";

    /// <summary>
    /// Xác định xem ứng dụng hiện tại có đang được khởi chạy với vai trò Custom Windows Shell hay không.
    /// Trả về true nếu ứng dụng được kích hoạt khi khởi động máy kèm tham số --kiosk-shell (hoặc explorer chưa chạy).
    /// Trả về false nếu người dùng mở ứng dụng thủ công/thông thường.
    /// </summary>
    bool IsRunningAsShell { get; }

    /// <summary>
    /// Kiểm tra và đăng ký đường dẫn file exe hiện tại làm Custom Shell
    /// trong nhánh HKCU Winlogon nếu chưa được thiết lập.
    /// </summary>
    void CheckAndInstallCustomShell();

    /// <summary>
    /// Khởi chạy lại Windows Shell mặc định (explorer.exe) để phục hồi Taskbar/Desktop,
    /// sau đó tắt ứng dụng an toàn.
    /// </summary>
    Task StartExplorerAndExitAsync();

    /// <summary>
    /// Khôi phục giá trị Shell về mặc định của Windows ("explorer.exe").
    /// </summary>
    void RestoreDefaultShell();
}
