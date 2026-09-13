namespace English.Entity.Services;

/// <summary>
/// Giao diện dành cho các ViewModel đóng vai trò là một Page / Screen trong hệ thống Navigator.
/// Cho phép ViewModel có thể tự yêu cầu đóng chính nó (quay lại trang trước) từ bên trong ViewModel logic.
/// </summary>
public interface IPageViewModel
{
    /// <summary>
    /// Action callback để thông báo cho NavigatorService đóng Page này (hoặc GoBack).
    /// NavigatorService sẽ tự động gán callback này khi Page được điều hướng tới.
    /// </summary>
    Action? RequestClose { get; set; }

    /// <summary>
    /// Đóng Page hiện tại và quay về màn hình trước đó trong PageStack.
    /// </summary>
    void Close();
}
