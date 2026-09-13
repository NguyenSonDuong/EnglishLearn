using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EnglishManager.Services;

public enum DialogType
{
    Info,
    Success,
    Warning,
    Error,
    Confirmation
}

public interface IInAppDialogService
{
    bool IsOpen { get; }
    string Title { get; }
    string Message { get; }
    DialogType Type { get; }
    bool IsConfirmation { get; }
    Task ShowInfoAsync(string title, string message);
    Task ShowSuccessAsync(string title, string message);
    Task ShowWarningAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task<bool> ShowConfirmAsync(string title, string message);
    void Close();
}

public partial class InAppDialogService : ObservableObject, IInAppDialogService
{
    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private DialogType _type = DialogType.Info;

    [ObservableProperty]
    private bool _isConfirmation;

    private TaskCompletionSource<bool>? _dialogTcs;

    public Task ShowInfoAsync(string title, string message)
    {
        return ShowModalAsync(title, message, DialogType.Info, false);
    }

    public Task ShowSuccessAsync(string title, string message)
    {
        return ShowModalAsync(title, message, DialogType.Success, false);
    }

    public Task ShowWarningAsync(string title, string message)
    {
        return ShowModalAsync(title, message, DialogType.Warning, false);
    }

    public Task ShowErrorAsync(string title, string message)
    {
        return ShowModalAsync(title, message, DialogType.Error, false);
    }

    public Task<bool> ShowConfirmAsync(string title, string message)
    {
        return ShowModalAsync(title, message, DialogType.Confirmation, true);
    }

    private Task<bool> ShowModalAsync(string title, string message, DialogType type, bool isConfirm)
    {
        _dialogTcs?.TrySetResult(false);
        _dialogTcs = new TaskCompletionSource<bool>();

        Title = title;
        Message = message;
        Type = type;
        IsConfirmation = isConfirm;
        IsOpen = true;

        return _dialogTcs.Task;
    }

    [RelayCommand]
    public void Confirm()
    {
        IsOpen = false;
        _dialogTcs?.TrySetResult(true);
    }

    [RelayCommand]
    public void Cancel()
    {
        IsOpen = false;
        _dialogTcs?.TrySetResult(false);
    }

    public void Close()
    {
        IsOpen = false;
        _dialogTcs?.TrySetResult(false);
    }
}
