using System.Collections.ObjectModel;
using System.Windows;
using English.Entity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace English.Service.Services;

/// <summary>
/// Triển khai INavigatorService: Điều phối trung chuyển hiển thị, mở, đóng và xếp chồng (Stack) các Dialog / UserControl.
/// 
/// Các tính năng then chốt:
/// 1. Mở màn hình bất kỳ chỉ cần truyền Class của ViewModel (OpenDialog&lt;TViewModel&gt;()).
/// 2. Hỗ trợ ViewModel tự đóng chính nó thông qua RequestClose callback hoặc Close() command.
/// 3. Truyền 1 hoặc nhiều tham số (Id, DTO, Action, Func callback) sang ViewModel nhận qua IParameterReceiver hoặc Action&lt;TViewModel&gt; configure.
/// 4. Tích hợp DI Container (IServiceProvider): tự động phân giải ViewModel cùng các Service phụ thuộc.
/// 5. Cơ chế LIFO: Dialog gọi sau cùng luôn được thêm vào đỉnh ngăn xếp (Z-order cao nhất trên giao diện).
/// 6. Hỗ trợ hiển thị dạng Stack để các dialog xếp lớp tự nhiên, khi đóng dialog trên cùng thì dialog bên dưới lập tức hiển thị lại.
/// </summary>
public class NavigatorService : INavigatorService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ngăn xếp các Dialog ViewModel đang hoạt động.
    /// Phần tử cuối cùng trong Collection chính là phần tử nằm trên cùng của màn hình.
    /// </summary>
    public ObservableCollection<object> DialogStack { get; } = new();

    /// <summary>
    /// ViewModel của Dialog đang nằm trên đỉnh ngăn xếp (Active Top Dialog).
    /// </summary>
    public object? CurrentDialog => DialogStack.Count > 0 ? DialogStack[^1] : null;

    /// <summary>
    /// Kiểm tra xem hiện có Dialog nào đang mở hay không.
    /// </summary>
    public bool HasActiveDialogs => DialogStack.Count > 0;

    /// <summary>
    /// Sự kiện thông báo khi trạng thái ngăn xếp Dialog thay đổi (mở thêm hoặc đóng).
    /// </summary>
    public event Action? DialogStackChanged;

    /// <summary>
    /// Khởi tạo NavigatorService với DI ServiceProvider để phân giải các ViewModel.
    /// </summary>
    /// <param name="serviceProvider">Dependency Injection provider.</param>
    public NavigatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Mở một Dialog theo ViewModel class (yêu cầu số 1 & 3).
    /// </summary>
    public TViewModel OpenDialog<TViewModel>(params object?[] parameters) where TViewModel : class
    {
        return OpenDialog<TViewModel>(null, parameters);
    }

    /// <summary>
    /// Mở một Dialog theo ViewModel class kèm delegate cấu hình khởi tạo (Action/Func) (yêu cầu số 1 & 3).
    /// </summary>
    public TViewModel OpenDialog<TViewModel>(Action<TViewModel>? configure, params object?[] parameters) where TViewModel : class
    {
        var instance = OpenDialogInternal(
            typeof(TViewModel),
            parameters,
            obj => configure?.Invoke((TViewModel)obj)
        );

        return (TViewModel)instance;
    }

    /// <summary>
    /// Mở một Dialog theo Type của ViewModel (yêu cầu số 1).
    /// </summary>
    public object OpenDialog(Type viewModelType, params object?[] parameters)
    {
        return OpenDialogInternal(viewModelType, parameters, null);
    }

    /// <summary>
    /// Logic cốt lõi phân giải ViewModel, gán RequestClose, truyền parameters và đưa vào Stack.
    /// </summary>
    private object OpenDialogInternal(Type viewModelType, object?[]? parameters, Action<object>? configure)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        // 1. Phân giải instance của ViewModel:
        //    Ưu tiên lấy từ DI Container (nếu đã đăng ký AddTransient / AddScoped).
        //    Nếu chưa đăng ký trong DI, dùng ActivatorUtilities để tự động inject các service phụ thuộc vào constructor.
        object? instance = _serviceProvider.GetService(viewModelType);
        if (instance == null)
        {
            try
            {
                instance = ActivatorUtilities.CreateInstance(_serviceProvider, viewModelType);
            }
            catch
            {
                // Fallback cuối cùng nếu không có constructor phù hợp qua ActivatorUtilities
                instance = Activator.CreateInstance(viewModelType)
                    ?? throw new InvalidOperationException($"Không thể tạo thể hiện của ViewModel '{viewModelType.FullName}'.");
            }
        }

        // 2. Thiết lập cơ chế tự đóng (yêu cầu số 2):
        //    Nếu ViewModel triển khai IDialogViewModel, gán RequestClose callback để nó có thể tự gọi Close().
        if (instance is IDialogViewModel dialogVm)
        {
            dialogVm.RequestClose = () => CloseDialog(instance);
        }

        // 3. Truyền mảng tham số sang ViewModel (yêu cầu số 3):
        //    Nếu ViewModel triển khai IParameterReceiver, chuyển tiếp toàn bộ tham số (Id, DTO, Action, Func...) sang.
        if (parameters != null && parameters.Length > 0 && instance is IParameterReceiver parameterReceiver)
        {
            parameterReceiver.ReceiveParameters(parameters);
        }

        // 4. Thực thi delegate cấu hình bổ sung (nếu có):
        //    Cho phép caller cấu hình trực tiếp các callback Action/Func hoặc gán dữ liệu ban đầu.
        configure?.Invoke(instance);

        // 5. Đưa Dialog vào ngăn xếp (yêu cầu số 5 & 6):
        //    Đảm bảo thao tác cập nhật Collection diễn ra trên UI Thread (Dispatcher).
        //    Phần tử thêm sau cùng sẽ nằm ở đỉnh ngăn xếp (Z-order cao nhất trên giao diện).
        ExecuteOnUIThread(() =>
        {
            if (DialogStack.Contains(instance))
            {
                // Nếu đã tồn tại trong stack, gỡ ra và đưa lên đầu đỉnh ngăn xếp
                DialogStack.Remove(instance);
            }

            DialogStack.Add(instance);
            DialogStackChanged?.Invoke();
        });

        return instance;
    }

    /// <summary>
    /// Đóng một Dialog cụ thể khỏi ngăn xếp (yêu cầu số 2).
    /// </summary>
    public bool CloseDialog(object viewModel)
    {
        if (viewModel == null) return false;

        bool removed = false;

        ExecuteOnUIThread(() =>
        {
            if (DialogStack.Contains(viewModel))
            {
                removed = DialogStack.Remove(viewModel);

                // Nếu ViewModel có implement IDisposable, giải phóng tài nguyên
                if (viewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                DialogStackChanged?.Invoke();
            }
        });

        return removed;
    }

    /// <summary>
    /// Đóng Dialog đang nằm trên đỉnh ngăn xếp (Dialog được mở gần đây nhất).
    /// </summary>
    public bool CloseTopDialog()
    {
        var top = CurrentDialog;
        return top != null && CloseDialog(top);
    }

    /// <summary>
    /// Đóng toàn bộ tất cả các Dialog đang mở trên giao diện, xóa sạch ngăn xếp.
    /// </summary>
    public void CloseAllDialogs()
    {
        ExecuteOnUIThread(() =>
        {
            if (DialogStack.Count == 0) return;

            var items = DialogStack.ToList();
            DialogStack.Clear();

            foreach (var item in items)
            {
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            DialogStackChanged?.Invoke();
        });
    }

    /// <summary>
    /// Hỗ trợ chạy an toàn trên Dispatcher của WPF UI thread.
    /// </summary>
    private static void ExecuteOnUIThread(Action action)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(action);
        }
        else
        {
            action();
        }
    }
}
