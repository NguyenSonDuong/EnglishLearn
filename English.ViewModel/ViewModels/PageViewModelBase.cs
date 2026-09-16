using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// Lớp cơ sở dành cho các ViewModel đại diện cho Page / UserControl trong hệ thống Single Page của MainWindow.
/// Kế thừa từ ObservableObject và triển khai sẵn IPageViewModel, IParameterReceiver.
/// 
/// Lợi ích khi kế thừa:
/// 1. Tự động liên kết với NavigatorService để tự đóng hoặc quay lại trang trước thông qua Close() / CloseCommand.
/// 2. Tiếp nhận tham số linh hoạt (DTO, Id, Action, callback...) thông qua ReceiveParameters.
/// 3. Sẵn sàng binding trực tiếp vào Button XAML với Command=""{Binding CloseCommand}"".
/// </summary>
public abstract partial class PageViewModelBase : ObservableObject, IPageViewModel, IParameterReceiver
{
    /// <summary>
    /// Tiêu đề hiển thị của Page (nếu UI cần bind).
    /// </summary>
    [ObservableProperty]
    private string _pageTitle = string.Empty;

    /// <summary>
    /// Callback được gán tự động bởi INavigatorService khi Page được kích hoạt.
    /// Kích hoạt callback này sẽ đóng Page và quay lại trang trước trong PageStack.
    /// </summary>
    public Action? RequestClose { get; set; }

    /// <summary>
    /// Đóng Page hiện tại và quay về trang trước đó.
    /// Có thể gọi trực tiếp từ C# code hoặc bind vào Button qua CloseCommand.
    /// </summary>
    [RelayCommand]
    public virtual void Close()
    {
        RequestClose?.Invoke();
    }

    /// <summary>
    /// Phương thức ảo tiếp nhận các tham số được truyền khi gọi Navigator.NavigateTo(...).
    /// Các lớp con có thể override để xử lý dữ liệu truyền sang.
    /// </summary>
    /// <param name="parameters">Danh sách tham số truyền vào.</param>
    public virtual void ReceiveParameters(params object?[] parameters)
    {
    }
}
