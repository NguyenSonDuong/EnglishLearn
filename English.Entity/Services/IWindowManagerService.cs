namespace English.Entity.Services;

/// <summary>
/// Service quản lý hiển thị các cửa sổ trên hệ thống đa màn hình (Multi-monitor).
/// </summary>
public interface IWindowManagerService
{
    /// <summary>
    /// Định vị và mở MainWindow trên màn hình chính,
    /// đồng thời mở các BlackoutWindow để che toàn bộ màn hình phụ.
    /// </summary>
    void OpenAllWindows();

    /// <summary>
    /// Cho phép đóng toàn bộ BlackoutWindow và MainWindow khi người dùng đã vượt qua thử thách.
    /// </summary>
    void UnlockAllScreens();
}
