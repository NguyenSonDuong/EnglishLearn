namespace English.Entity.Services;

/// <summary>
/// Giao diện dành cho các ViewModel đóng vai trò là một Dialog / Screen trong hệ thống Navigator.
/// Cho phép ViewModel có thể tự yêu cầu đóng chính nó từ bên trong ViewModel logic (yêu cầu số 2).
/// </summary>
public interface IDialogViewModel
{
    /// <summary>
    /// Action callback để thông báo cho NavigatorService đóng Dialog này.
    /// NavigatorService sẽ tự động gán callback này khi Dialog được mở.
    /// </summary>
    Action? RequestClose { get; set; }

    /// <summary>
    /// Phương thức hỗ trợ đóng Dialog trực tiếp từ bên trong ViewModel.
    /// Triển khai mặc định thường sẽ kích hoạt RequestClose?.Invoke().
    /// </summary>
    void Close();
}
