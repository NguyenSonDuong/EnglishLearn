using System.Diagnostics;
using System.IO;
using English.Entity.Services;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace English.Service.Services;

/// <summary>
/// Triển khai dịch vụ quản lý Custom Shell thông qua Windows Registry (HKCU).
/// 
/// 💡 CƠ CHẾ CUSTOM SHELL:
///   - Windows đọc giá trị HKCU\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell
///     ngay sau khi người dùng đăng nhập.
///   - Mặc định giá trị này không tồn tại (hệ thống dùng HKLM = "explorer.exe").
///   - Khi gán Shell = đường dẫn_EnglishLocker.exe, Windows sẽ chạy ứng dụng này thay vì explorer.exe,
///     đồng nghĩa với việc KHÔNG CÓ Desktop, KHÔNG CÓ Taskbar và Start Menu.
///   - Sử dụng HKCU (Current User) an toàn hơn HKLM vì chỉ áp dụng cho tài khoản hiện tại,
///     không yêu cầu quyền Administrator và dễ dàng khôi phục.
/// </summary>
public class ShellManagementService : IShellManagementService
{
    /// <summary>
    /// Đường dẫn Registry của Winlogon trong HKEY_CURRENT_USER.
    /// </summary>
    private const string WinlogonSubKey = @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon";

    /// <summary>
    /// Tên Value quy định Shell khởi động cùng phiên đăng nhập người dùng.
    /// </summary>
    private const string ShellValueName = "Shell";

    /// <summary>
    /// Tên tiến trình Shell mặc định của Windows.
    /// </summary>
    private const string DefaultShell = "explorer.exe";

    /// <summary>
    /// Nhận diện xem ứng dụng có đang chạy với tư cách Windows Shell (được kích hoạt với cờ --kiosk-shell) hay không.
    /// </summary>
    public bool IsRunningAsShell { get; } = Environment.GetCommandLineArgs()
        .Any(arg => string.Equals(arg, IShellManagementService.KioskShellArgument, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Kiểm tra và thiết lập file thực thi hiện tại làm Custom Shell của tài khoản kèm tham số --kiosk-shell.
    /// </summary>
    public void CheckAndInstallCustomShell()
    {
#if !DEBUG
        try
        {
            // Lấy đường dẫn tuyệt đối của file thực thi (.exe) hiện tại
            string currentExePath = Process.GetCurrentProcess().MainModule?.FileName 
                                    ?? Environment.ProcessPath 
                                    ?? string.Empty;

            if (string.IsNullOrWhiteSpace(currentExePath) || !File.Exists(currentExePath))
            {
                Debug.WriteLine("[ShellManagement] Không thể xác định đường dẫn file thực thi hiện tại.");
                return;
            }

            // Lệnh đăng ký Shell: bao gồm đường dẫn bọc nháy kép và tham số --kiosk-shell
            string expectedShellCommand = $"\"{currentExePath}\" {IShellManagementService.KioskShellArgument}";

            // Mở hoặc tạo nhánh Registry trong HKCU
            using var key = Registry.CurrentUser.CreateSubKey(WinlogonSubKey, writable: true);
            if (key == null)
            {
                Debug.WriteLine("[ShellManagement] Không thể mở hoặc tạo key Winlogon trong Registry.");
                return;
            }

            var currentShellValue = key.GetValue(ShellValueName) as string;

            // Nếu giá trị chưa có hoặc khác với câu lệnh shell kèm tham số thì cập nhật
            if (!string.Equals(currentShellValue, expectedShellCommand, StringComparison.OrdinalIgnoreCase))
            {
                key.SetValue(ShellValueName, expectedShellCommand, RegistryValueKind.String);
                Debug.WriteLine($"[ShellManagement] Đã đăng ký Custom Shell thành công: '{expectedShellCommand}'");
            }
            else
            {
                Debug.WriteLine("[ShellManagement] Custom Shell đã được cấu hình trước đó với cờ tham số, không cần thay đổi.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"[ShellManagement] Quyền truy cập bị từ chối khi ghi Registry: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShellManagement] Lỗi khi cài đặt Custom Shell: {ex.Message}");
        }
#else
        RestoreDefaultShell();
#endif
    }

    /// <summary>
    /// Khởi động lại Windows Explorer (nạp lại Desktop, Taskbar) và đóng EnglishLocker.
    /// </summary>
    public async Task StartExplorerAndExitAsync()
    {
        try
        {
            // Kiểm tra xem explorer.exe đã chạy chưa
            bool isExplorerRunning = Process.GetProcessesByName("explorer").Length > 0;

            if (!isExplorerRunning || IsRunningAsShell)
            {
                Debug.WriteLine("[ShellManagement] Đang kích hoạt Windows Explorer...");

                // Lấy đường dẫn tuyệt đối của explorer.exe từ thư mục Windows
                string explorerPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "explorer.exe");

                // Khởi chạy tiến trình explorer.exe độc lập với hệ điều hành
                var startInfo = new ProcessStartInfo
                {
                    FileName = explorerPath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);

                Debug.WriteLine("[ShellManagement] Windows Explorer đã được khởi động thành công.");
            }
            else
            {
                Debug.WriteLine("[ShellManagement] Mở thông thường và Explorer đã chạy sẵn, bỏ qua việc khởi chạy lại Explorer.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShellManagement] Lỗi khi gọi StartExplorer: {ex.Message}");
        }

        // Đợi 1.5 giây để hệ thống có đủ thời gian spin-up tiến trình Explorer (nạp Taskbar, Desktop)
        await Task.Delay(1500);

        // Đóng ứng dụng an toàn
        if (Application.Current != null)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Shutdown();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            }
        }
    }

    /// <summary>
    /// Khôi phục Registry về giá trị "explorer.exe" mặc định (dùng khi gỡ bỏ hoặc bảo trì).
    /// </summary>
    public void RestoreDefaultShell()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(WinlogonSubKey, writable: true);
            if (key != null)
            {
                // Gán lại thành "explorer.exe" hoặc xóa Value để Windows dùng mặc định
                key.SetValue(ShellValueName, DefaultShell, RegistryValueKind.String);
                Debug.WriteLine("[ShellManagement] Đã khôi phục Shell mặc định (explorer.exe) thành công.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShellManagement] Lỗi khi khôi phục Shell mặc định: {ex.Message}");
        }
    }
}
