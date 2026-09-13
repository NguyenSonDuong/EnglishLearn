namespace English.Entity.Services;

/// <summary>
/// Interface cho các thao tác hệ thống: khóa/mở Task Manager, tắt máy.
/// </summary>
public interface ISystemControlService
{
    /// <summary>Vô hiệu hóa Task Manager thông qua Registry.</summary>
    void DisableTaskManager();

    /// <summary>Kích hoạt lại Task Manager.</summary>
    void EnableTaskManager();

    /// <summary>Ra lệnh tắt máy tính (phạt gian lận).</summary>
    void ShutdownComputer();
}
