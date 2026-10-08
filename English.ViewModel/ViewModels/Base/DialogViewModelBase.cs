using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// Lớp cơ sở dành cho các ViewModel đại diện cho Dialog / Popup / Screen trong hệ thống Navigator.
/// Kế thừa từ ObservableObject và triển khai sẵn IDialogViewModel, IParameterReceiver.
/// 
/// Lợi ích khi kế thừa lớp này:
/// 1. Tự động liên kết với NavigatorService để tự đóng chính nó thông qua lệnh Close() hoặc CloseCommand.
/// 2. Hỗ trợ nhận các tham số (params object?[]) khi được mở thông qua phương thức ảo ReceiveParameters.
/// 3. Sẵn sàng binding trực tiếp vào Button XAML với Command="{Binding CloseCommand}".
/// </summary>
public abstract partial class DialogViewModelBase : ObservableObject, IDialogViewModel, IParameterReceiver
{
    /// <summary>
    /// Tiêu đề hiển thị của Dialog (nếu UI cần bind).
    /// </summary>
    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    /// <summary>
    /// Callback được gán tự động bởi INavigatorService khi Dialog được mở.
    /// Kích hoạt callback này sẽ gỡ ViewModel này khỏi DialogStack.
    /// </summary>
    public Action? RequestClose { get; set; }

    /// <summary>
    /// Đóng Dialog hiện tại (yêu cầu số 2).
    /// Có thể gọi trực tiếp từ code C# hoặc bind vào Button giao diện qua CloseCommand.
    /// Các lớp con có thể override lại nếu muốn kiểm tra điều kiện (validation) trước khi đóng.
    /// </summary>
    [RelayCommand]
    public virtual void Close()
    {
        RequestClose?.Invoke();
    }

    /// <summary>
    /// Phương thức ảo tiếp nhận các tham số được truyền khi gọi Navigator.OpenDialog(...) (yêu cầu số 3).
    /// Các lớp con có thể override để giải nén tham số (Model, Id, Action, Func callback...).
    /// </summary>
    /// <param name="parameters">Danh sách tham số truyền vào.</param>
    public virtual void ReceiveParameters(params object?[] parameters)
    {
        // Lớp con có thể ghi đè phương thức này để tiếp nhận tham số theo ý muốn.
    }
}
