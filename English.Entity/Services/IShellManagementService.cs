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
