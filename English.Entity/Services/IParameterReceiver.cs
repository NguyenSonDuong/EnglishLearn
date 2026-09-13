namespace English.Entity.Services;

/// <summary>
/// Giao diện cho phép ViewModel tiếp nhận 1 hoặc nhiều tham số khi được mở bởi NavigatorService (yêu cầu số 3).
/// Tham số có thể là bất kỳ đối tượng nào: Id, Model, DTO, Action, Func, callback, v.v.
/// </summary>
public interface IParameterReceiver
{
    /// <summary>
    /// Tiếp nhận mảng tham số truyền từ NavigatorService.OpenDialog(...).
    /// </summary>
    /// <param name="parameters">Danh sách tham số đa dạng được truyền vào.</param>
    void ReceiveParameters(params object?[] parameters);
}
