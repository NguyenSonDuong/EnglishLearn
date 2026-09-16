using English.Entity.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace English.Service.Services;

/// <summary>
/// Implementation thật của ISystemControlService.
/// ⚠ BẢN TEST: Tất cả thao tác can thiệp hệ thống đều chỉ log/MessageBox,
///   không thực sự thay đổi Registry hay tắt máy.
/// 
/// Khi triển khai thật, sẽ dùng:
///   - Registry: HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\System
///     → DisableTaskMgr = 1 (khóa) / xóa key (mở)
///   - shutdown.exe /s /f /t 0
/// </summary>
public class SystemControlService : ISystemControlService
{
    /// <summary>
    /// Khóa Task Manager bằng cách ghi Registry key DisableTaskMgr = 1.
    /// (Bản test: chỉ log ra Debug)
    /// </summary>
    public void DisableTaskManager()
    {
#if DEBUG
        Debug.WriteLine("[SystemControl] Chế độ Debug: Bỏ qua vô hiệu hóa Task Manager.");
#else
         using var key = Registry.CurrentUser.CreateSubKey(
             @"Software\Microsoft\Windows\CurrentVersion\Policies\System");
         key?.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);

        Debug.WriteLine("[SystemControl] DisableTaskManager() → Task Manager đã bị khóa (mock).");
#endif
    }

    /// <summary>
    /// Mở lại Task Manager bằng cách xóa Registry key DisableTaskMgr.
    /// (Bản test: chỉ log ra Debug)
    /// </summary>
    public void EnableTaskManager()
    {
#if !DEBUG
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Policies\System", writable: true);
        key?.DeleteValue("DisableTaskMgr", throwOnMissingValue: false);
#else
        Debug.WriteLine("[SystemControl] EnableTaskManager() → Task Manager đã được mở khóa (mock).");
#endif
    }

    /// <summary>
    /// Tắt máy tính ngay lập tức (phạt gian lận).
    /// (Bản test: chỉ hiện MessageBox cảnh báo)
    /// </summary>
    public void ShutdownComputer()
    {
#if !DEBUG
         Process.Start(new ProcessStartInfo
         {
             FileName = "shutdown.exe",
             Arguments = "/s /f /t 0",   // /s = shutdown, /f = force, /t 0 = ngay lập tức
             CreateNoWindow = true,
             UseShellExecute = false
         });
#else
        Debug.WriteLine("[SystemControl] ShutdownComputer() → Đã gọi lệnh tắt máy (mock).");
        MessageBox.Show(
            "⚠ CẢNH BÁO: Hệ thống sẽ tắt máy do phát hiện gian lận!\n(Bản test – không tắt thật)",
            "EnglishLocker – Shutdown",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
#endif
    }
}
