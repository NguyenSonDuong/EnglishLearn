namespace English.Entity.Enums;

/// <summary>
/// Xác định chiều di chuyển animation khi thực hiện chuyển đổi giữa các trang (Page / UserControl).
/// </summary>
public enum NavigationTransitionDirection
{
    /// <summary>Chuyển tới trang mới (Forward navigation, trượt từ phải sang trái hoặc mở mới).</summary>
    Forward,

    /// <summary>Quay lại trang trước (Backward navigation / GoBack, trượt từ trái sang phải hoặc đóng lại).</summary>
    Backward,

    /// <summary>Không áp dụng animation định hướng hoặc chỉ fade nhẹ nhàng.</summary>
    None
}
