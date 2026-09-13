using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using English.Entity.Enums;
using English.Entity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace English.Service.Services;

/// <summary>
/// Triển khai INavigatorService: Điều phối trung chuyển hiển thị, mở, đóng và chuyển đổi các UserControl / Page và Dialog.
/// 
/// Tính năng then chốt:
/// 1. Điều phối Page navigation trên Single Page container, quản lý PageStack và CurrentPage.
/// 2. Hỗ trợ tham số addToStack (mặc định true). Nếu false, Page mới chỉ được hiển thị mà không lưu vào PageStack.
/// 3. Cung cấp thuộc tính TransitionDirection (Forward, Backward) và kích hoạt sự kiện để View thực hiện animation chuyển trang mượt mà.
/// 4. Hỗ trợ cơ chế GoBack() và đóng Page tự động từ bên trong ViewModel qua IPageViewModel.RequestClose.
/// 5. Quản lý DialogStack dạng LIFO Modal Overlay trên cùng giao diện.
/// 6. Tích hợp DI Container (IServiceProvider) và tự động truyền tham số đa dạng qua IParameterReceiver.
/// </summary>
public class NavigatorService : INavigatorService, INotifyPropertyChanged
{
    private readonly IServiceProvider _serviceProvider;

    private object? _currentPage;
    private NavigationTransitionDirection _transitionDirection = NavigationTransitionDirection.None;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ═══════════════════════════════════════════════════════════════════════
    // PAGE NAVIGATION & STACK MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ngăn xếp lịch sử các Page ViewModel đang được lưu trữ.
    /// </summary>
    public ObservableCollection<object> PageStack { get; } = new();

    /// <summary>
    /// ViewModel của Page hiện đang được hiển thị trên giao diện chính.
    /// </summary>
    public object? CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (!Equals(_currentPage, value))
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(CanGoBack));
                CurrentPageChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Cho biết hiện tại có thể thực hiện thao tác quay lại trang trước hay không.
    /// </summary>
    public bool CanGoBack
    {
        get
        {
            if (CurrentPage == null) return false;

            // Nếu CurrentPage không nằm trong PageStack (do mở với addToStack = false),
            // có thể quay lại nếu PageStack có ít nhất 1 phần tử.
            if (!PageStack.Contains(CurrentPage))
            {
                return PageStack.Count > 0;
            }

            // Nếu CurrentPage nằm trong PageStack, chỉ có thể quay lại nếu stack có từ 2 phần tử trở lên.
            return PageStack.Count > 1;
        }
    }

    /// <summary>
    /// Chiều chuyển động animation hiện tại khi chuyển đổi giữa các Page.
    /// </summary>
    public NavigationTransitionDirection TransitionDirection
    {
        get => _transitionDirection;
        private set
        {
            if (_transitionDirection != value)
            {
                _transitionDirection = value;
                OnPropertyChanged(nameof(TransitionDirection));
            }
        }
    }

    public event Action? CurrentPageChanged;
    public event Action? PageStackChanged;

    public NavigatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Điều hướng tới Page mới theo ViewModel class (addToStack mặc định là true).
    /// </summary>
    public TViewModel NavigateTo<TViewModel>(bool addToStack = true, params object?[] parameters) where TViewModel : class
    {
        return NavigateTo<TViewModel>(null, addToStack, parameters);
    }

    /// <summary>
    /// Điều hướng tới Page mới kèm delegate cấu hình.
    /// </summary>
    public TViewModel NavigateTo<TViewModel>(Action<TViewModel>? configure, bool addToStack = true, params object?[] parameters) where TViewModel : class
    {
        var instance = NavigateToInternal(
            typeof(TViewModel),
            addToStack,
            parameters,
            obj => configure?.Invoke((TViewModel)obj)
        );

        return (TViewModel)instance;
    }

    /// <summary>
    /// Điều hướng tới Page mới theo Type của ViewModel.
    /// </summary>
    public object NavigateTo(Type viewModelType, bool addToStack = true, params object?[] parameters)
    {
        return NavigateToInternal(viewModelType, addToStack, parameters, null);
    }

    /// <summary>
    /// Logic cốt lõi phân giải ViewModel, gán RequestClose, truyền parameters và cập nhật PageStack/CurrentPage.
    /// </summary>
    private object NavigateToInternal(Type viewModelType, bool addToStack, object?[]? parameters, Action<object>? configure)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        object instance = ResolveViewModelInstance(viewModelType);

        // Thiết lập cơ chế tự đóng / quay lại cho Page
        if (instance is IPageViewModel pageVm)
        {
            pageVm.RequestClose = () => ClosePage(instance);
        }

        // Truyền mảng tham số nếu ViewModel có nhận tham số
        if (parameters != null && parameters.Length > 0 && instance is IParameterReceiver parameterReceiver)
        {
            parameterReceiver.ReceiveParameters(parameters);
        }

        // Thực thi cấu hình bổ sung
        configure?.Invoke(instance);

        // Cập nhật giao diện trên UI Thread
        ExecuteOnUIThread(() =>
        {
            TransitionDirection = NavigationTransitionDirection.Forward;

            if (addToStack)
            {
                if (PageStack.Contains(instance))
                {
                    PageStack.Remove(instance);
                }

                PageStack.Add(instance);
                PageStackChanged?.Invoke();
            }

            CurrentPage = instance;
        });

        return instance;
    }

    /// <summary>
    /// Quay lại Page trước đó trong ngăn xếp.
    /// </summary>
    public bool GoBack()
    {
        if (!CanGoBack) return false;

        bool handled = false;

        ExecuteOnUIThread(() =>
        {
            TransitionDirection = NavigationTransitionDirection.Backward;

            if (CurrentPage != null && !PageStack.Contains(CurrentPage))
            {
                // CurrentPage không nằm trong PageStack (được mở với addToStack = false)
                if (CurrentPage is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                CurrentPage = PageStack.Count > 0 ? PageStack[^1] : null;
                handled = true;
            }
            else if (PageStack.Count > 1)
            {
                // CurrentPage nằm trên đỉnh PageStack
                var leavingPage = PageStack[^1];
                PageStack.RemoveAt(PageStack.Count - 1);

                if (leavingPage is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                CurrentPage = PageStack[^1];
                PageStackChanged?.Invoke();
                handled = true;
            }
        });

        return handled;
    }

    /// <summary>
    /// Đóng một Page cụ thể khỏi hệ thống.
    /// </summary>
    public bool ClosePage(object viewModel)
    {
        if (viewModel == null) return false;

        bool removed = false;

        ExecuteOnUIThread(() =>
        {
            if (Equals(viewModel, CurrentPage))
            {
                if (CanGoBack)
                {
                    removed = GoBack();
                }
                else
                {
                    if (PageStack.Contains(viewModel))
                    {
                        PageStack.Remove(viewModel);
                        PageStackChanged?.Invoke();
                    }

                    if (viewModel is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    CurrentPage = PageStack.Count > 0 ? PageStack[^1] : null;
                    removed = true;
                }
            }
            else if (PageStack.Contains(viewModel))
            {
                removed = PageStack.Remove(viewModel);

                if (viewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                PageStackChanged?.Invoke();
                OnPropertyChanged(nameof(CanGoBack));
            }
        });

        return removed;
    }

    /// <summary>
    /// Xóa toàn bộ lịch sử các Page trong ngăn xếp.
    /// </summary>
    public void ClearPageStack()
    {
        ExecuteOnUIThread(() =>
        {
            if (PageStack.Count == 0) return;

            var items = PageStack.ToList();
            PageStack.Clear();

            foreach (var item in items)
            {
                if (!Equals(item, CurrentPage) && item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            PageStackChanged?.Invoke();
            OnPropertyChanged(nameof(CanGoBack));
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DIALOG NAVIGATION & OVERLAY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ngăn xếp các Dialog ViewModel đang hoạt động.
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
    /// Mở một Dialog theo ViewModel class.
    /// </summary>
    public TViewModel OpenDialog<TViewModel>(params object?[] parameters) where TViewModel : class
    {
        return OpenDialog<TViewModel>(null, parameters);
    }

    /// <summary>
    /// Mở một Dialog theo ViewModel class kèm delegate cấu hình khởi tạo.
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
    /// Mở một Dialog theo Type của ViewModel.
    /// </summary>
    public object OpenDialog(Type viewModelType, params object?[] parameters)
    {
        return OpenDialogInternal(viewModelType, parameters, null);
    }

    /// <summary>
    /// Logic phân giải và đẩy Dialog lên đỉnh ngăn xếp DialogStack.
    /// </summary>
    private object OpenDialogInternal(Type viewModelType, object?[]? parameters, Action<object>? configure)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        object instance = ResolveViewModelInstance(viewModelType);

        if (instance is IDialogViewModel dialogVm)
        {
            dialogVm.RequestClose = () => CloseDialog(instance);
        }

        if (parameters != null && parameters.Length > 0 && instance is IParameterReceiver parameterReceiver)
        {
            parameterReceiver.ReceiveParameters(parameters);
        }

        configure?.Invoke(instance);

        ExecuteOnUIThread(() =>
        {
            if (DialogStack.Contains(instance))
            {
                DialogStack.Remove(instance);
            }

            DialogStack.Add(instance);
            OnPropertyChanged(nameof(CurrentDialog));
            OnPropertyChanged(nameof(HasActiveDialogs));
            DialogStackChanged?.Invoke();
        });

        return instance;
    }

    /// <summary>
    /// Đóng một Dialog cụ thể khỏi ngăn xếp.
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

                if (viewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                OnPropertyChanged(nameof(CurrentDialog));
                OnPropertyChanged(nameof(HasActiveDialogs));
                DialogStackChanged?.Invoke();
            }
        });

        return removed;
    }

    /// <summary>
    /// Đóng Dialog đang nằm trên đỉnh ngăn xếp.
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

            OnPropertyChanged(nameof(CurrentDialog));
            OnPropertyChanged(nameof(HasActiveDialogs));
            DialogStackChanged?.Invoke();
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Phân giải instance của ViewModel từ DI Container hoặc ActivatorUtilities.
    /// </summary>
    private object ResolveViewModelInstance(Type viewModelType)
    {
        object? instance = _serviceProvider.GetService(viewModelType);
        if (instance != null) return instance;

        try
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, viewModelType);
        }
        catch
        {
            return Activator.CreateInstance(viewModelType)
                ?? throw new InvalidOperationException($"Không thể tạo thể hiện của ViewModel '{viewModelType.FullName}'.");
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
