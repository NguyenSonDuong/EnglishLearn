using System.Collections.ObjectModel;
using English.Entity.Enums;

namespace English.Entity.Services;

/// <summary>
/// Service điều phối trung chuyển mở, đóng và quản lý các màn hình UserControl / Page và Dialog trên giao diện.
/// Đáp ứng các yêu cầu:
/// 1. Quản lý hệ thống chuyển trang (Page navigation) trên MainWindow dạng Single Page.
/// 2. Hỗ trợ cơ chế Stack (ngăn xếp) cho các Page, cho phép GoBack / quay lại trang trước.
/// 3. Tham số addToStack: Mặc định là true (lưu vào ngăn xếp), nếu là false thì trang chỉ hiển thị nhưng không add vào stack.
/// 4. Quản lý DialogStack và hiển thị Modal Overlay trên cùng giao diện.
/// 5. Điều hướng theo ViewModel class, tự động resolve từ DI và truyền parameters linh hoạt.
/// 6. Cung cấp thông tin TransitionDirection để UI kích hoạt hiệu ứng chuyển đổi mượt mà.
/// </summary>
public interface INavigatorService
{
    // ═══════════════════════════════════════════════════════════════════════
    // PAGE NAVIGATION & STACK MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ngăn xếp lịch sử các Page ViewModel đang được lưu trữ.
    /// Phần tử cuối cùng là trang nằm trên cùng của ngăn xếp lịch sử.
    /// </summary>
    ObservableCollection<object> PageStack { get; }

    /// <summary>
    /// ViewModel của Page hiện đang được hiển thị trực tiếp trên giao diện chính.
    /// </summary>
    object? CurrentPage { get; }

    /// <summary>
    /// Cho biết hiện tại có thể thực hiện thao tác quay lại trang trước (GoBack) hay không.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Chiều chuyển động animation hiện tại (Forward / Backward / None) cho chuyển đổi giữa các Page.
    /// </summary>
    NavigationTransitionDirection TransitionDirection { get; }

    /// <summary>
    /// Sự kiện phát ra khi CurrentPage thay đổi.
    /// </summary>
    event Action? CurrentPageChanged;

    /// <summary>
    /// Sự kiện phát ra khi ngăn xếp PageStack có sự thay đổi.
    /// </summary>
    event Action? PageStackChanged;

    /// <summary>
    /// Điều hướng tới một Page mới theo ViewModel class.
    /// </summary>
    /// <typeparam name="TViewModel">Loại ViewModel của Page cần hiển thị.</typeparam>
    /// <param name="addToStack">
    /// Nếu true (mặc định), Page mới sẽ được thêm vào PageStack để hỗ trợ quay lại.
    /// Nếu false, Page mới chỉ được hiển thị trên giao diện mà không lưu vào PageStack.
    /// </param>
    /// <param name="parameters">Mảng các tham số tùy chọn truyền sang Page.</param>
    /// <returns>Instance của TViewModel vừa được kích hoạt.</returns>
    TViewModel NavigateTo<TViewModel>(bool addToStack = true, params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Điều hướng tới một Page mới kèm delegate cấu hình khởi tạo.
    /// </summary>
    /// <typeparam name="TViewModel">Loại ViewModel của Page cần hiển thị.</typeparam>
    /// <param name="configure">Delegate cấu hình các thuộc tính hoặc callback trước khi hiển thị.</param>
    /// <param name="addToStack">Có lưu Page vào ngăn xếp hay không (mặc định true).</param>
    /// <param name="parameters">Mảng các tham số tùy chọn truyền sang Page.</param>
    /// <returns>Instance của TViewModel vừa được kích hoạt.</returns>
    TViewModel NavigateTo<TViewModel>(Action<TViewModel>? configure, bool addToStack = true, params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Điều hướng tới một Page mới theo Type của ViewModel (Non-generic overload).
    /// </summary>
    /// <param name="viewModelType">Type của ViewModel.</param>
    /// <param name="addToStack">Có lưu Page vào ngăn xếp hay không (mặc định true).</param>
    /// <param name="parameters">Mảng các tham số tùy chọn truyền sang Page.</param>
    /// <returns>Instance của ViewModel vừa được kích hoạt.</returns>
    object NavigateTo(Type viewModelType, bool addToStack = true, params object?[] parameters);

    /// <summary>
    /// Quay lại Page trước đó trong ngăn xếp (LIFO).
    /// </summary>
    /// <returns>True nếu quay lại thành công, False nếu không còn trang nào để quay lại.</returns>
    bool GoBack();

    /// <summary>
    /// Đóng một Page cụ thể. Nếu đó là trang đang hiển thị, sẽ quay về trang liền trước.
    /// </summary>
    /// <param name="viewModel">Thể hiện của Page ViewModel cần đóng.</param>
    /// <returns>True nếu đóng thành công, False nếu không tìm thấy.</returns>
    bool ClosePage(object viewModel);

    /// <summary>
    /// Xóa toàn bộ lịch sử các Page trong ngăn xếp.
    /// </summary>
    void ClearPageStack();


    // ═══════════════════════════════════════════════════════════════════════
    // DIALOG NAVIGATION & OVERLAY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ngăn xếp chứa các ViewModel của các Dialog/Screen đang được mở trên tầng overlay.
    /// Phần tử nằm cuối danh sách tương ứng với Dialog nằm trên cùng của Stack.
    /// </summary>
    ObservableCollection<object> DialogStack { get; }

    /// <summary>
    /// ViewModel của Dialog hiện tại đang nằm trên cùng màn hình (Active Top Dialog).
    /// </summary>
    object? CurrentDialog { get; }

    /// <summary>
    /// Cho biết hiện tại có ít nhất một Dialog đang mở trong ngăn xếp hay không.
    /// </summary>
    bool HasActiveDialogs { get; }

    /// <summary>
    /// Sự kiện phát ra khi ngăn xếp Dialog có sự thay đổi (mở thêm hoặc đóng).
    /// </summary>
    event Action? DialogStackChanged;

    /// <summary>
    /// Mở một Dialog theo ViewModel class.
    /// </summary>
    TViewModel OpenDialog<TViewModel>(params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Mở một Dialog theo ViewModel class kèm delegate cấu hình ban đầu.
    /// </summary>
    TViewModel OpenDialog<TViewModel>(Action<TViewModel>? configure, params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Mở một Dialog theo Type của ViewModel (Non-generic overload).
    /// </summary>
    object OpenDialog(Type viewModelType, params object?[] parameters);

    /// <summary>
    /// Đóng một Dialog cụ thể khỏi ngăn xếp.
    /// </summary>
    bool CloseDialog(object viewModel);

    /// <summary>
    /// Đóng Dialog đang nằm trên đỉnh ngăn xếp.
    /// </summary>
    bool CloseTopDialog();

    /// <summary>
    /// Đóng toàn bộ tất cả các Dialog đang mở trên giao diện, xóa sạch ngăn xếp.
    /// </summary>
    void CloseAllDialogs();
}
