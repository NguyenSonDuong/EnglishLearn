using System.Collections.ObjectModel;

namespace English.Entity.Services;

/// <summary>
/// Service điều phối trung chuyển mở, đóng và quản lý các màn hình UserControl / Dialog trên giao diện.
/// Đáp ứng các yêu cầu:
/// 1. Mở màn hình chỉ cần truyền ViewModel class.
/// 2. Hỗ trợ ViewModel tự đóng chính nó.
/// 3. Hỗ trợ truyền 1 hoặc nhiều tham số (Id, DTO, Action, Func callback...).
/// 4. Tối ưu sử dụng với DI (Dependency Injection).
/// 5. Dialog gọi sau cùng luôn hiển thị trên cùng (Z-order cao nhất).
/// 6. Hỗ trợ cơ chế Stack (ngăn xếp) để xếp chồng các màn hình lên nhau.
/// </summary>
public interface INavigatorService
{
    /// <summary>
    /// Ngăn xếp chứa các ViewModel của các Dialog/Screen đang được mở.
    /// Phần tử nằm cuối danh sách (chỉ số cao nhất) tương ứng với Dialog nằm trên cùng của Stack.
    /// </summary>
    ObservableCollection<object> DialogStack { get; }

    /// <summary>
    /// ViewModel của Dialog hiện tại đang nằm trên cùng màn hình (Active Top Dialog).
    /// Trả về null nếu hiện không có Dialog nào mở.
    /// </summary>
    object? CurrentDialog { get; }

    /// <summary>
    /// Cho biết hiện tại có ít nhất một Dialog đang mở trong ngăn xếp hay không.
    /// </summary>
    bool HasActiveDialogs { get; }

    /// <summary>
    /// Sự kiện phát ra khi ngăn xếp Dialog có sự thay đổi (mở thêm hoặc đóng).
    /// View hoặc MainViewModel có thể lắng nghe để cập nhật UI/Overlay mờ.
    /// </summary>
    event Action? DialogStackChanged;

    /// <summary>
    /// Mở một Dialog theo ViewModel class (yêu cầu số 1).
    /// Tự động resolve ViewModel từ DI Container, gán RequestClose và truyền parameters (yêu cầu số 3, 4).
    /// Đẩy Dialog mới lên đỉnh ngăn xếp để hiển thị trên cùng (yêu cầu số 5, 6).
    /// </summary>
    /// <typeparam name="TViewModel">Loại ViewModel của màn hình cần mở.</typeparam>
    /// <param name="parameters">Mảng các tham số tùy chọn (Model, Id, Action, Func callback...).</param>
    /// <returns>Instance của TViewModel vừa được mở.</returns>
    TViewModel OpenDialog<TViewModel>(params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Mở một Dialog theo ViewModel class kèm delegate cấu hình ban đầu (Action/Func).
    /// </summary>
    /// <typeparam name="TViewModel">Loại ViewModel cần mở.</typeparam>
    /// <param name="configure">Delegate cấu hình các thuộc tính hoặc callback trên ViewModel trước khi đưa lên stack.</param>
    /// <param name="parameters">Mảng các tham số khác tùy chọn.</param>
    /// <returns>Instance của TViewModel vừa được mở.</returns>
    TViewModel OpenDialog<TViewModel>(Action<TViewModel>? configure, params object?[] parameters) where TViewModel : class;

    /// <summary>
    /// Mở một Dialog theo Type của ViewModel (Non-generic overload).
    /// </summary>
    /// <param name="viewModelType">Type của ViewModel.</param>
    /// <param name="parameters">Mảng các tham số tùy chọn.</param>
    /// <returns>Instance của ViewModel vừa được mở.</returns>
    object OpenDialog(Type viewModelType, params object?[] parameters);

    /// <summary>
    /// Đóng một Dialog cụ thể khỏi ngăn xếp (yêu cầu số 2).
    /// </summary>
    /// <param name="viewModel">Thể hiện của ViewModel cần đóng.</param>
    /// <returns>True nếu đóng thành công, False nếu không tìm thấy trong ngăn xếp.</returns>
    bool CloseDialog(object viewModel);

    /// <summary>
    /// Đóng Dialog đang nằm trên đỉnh ngăn xếp (Dialog được mở gần đây nhất).
    /// </summary>
    /// <returns>True nếu đóng thành công, False nếu ngăn xếp rỗng.</returns>
    bool CloseTopDialog();

    /// <summary>
    /// Đóng toàn bộ tất cả các Dialog đang mở trên giao diện, xóa sạch ngăn xếp.
    /// </summary>
    void CloseAllDialogs();
}
