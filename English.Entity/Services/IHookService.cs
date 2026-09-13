namespace English.Entity.Services;

/// <summary>
/// Interface cho service hook bàn phím cấp thấp (Low-Level Keyboard Hook).
/// </summary>
public interface IHookService : IDisposable
{
    /// <summary>Cài đặt hook để chặn các tổ hợp phím nguy hiểm.</summary>
    void InstallHook();

    /// <summary>Gỡ bỏ hook, trả quyền điều khiển phím cho hệ thống.</summary>
    void UninstallHook();
}
