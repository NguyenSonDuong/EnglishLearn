using System.Windows;
using English.ViewModel.ViewModels;

namespace EnglishLocker.Views;

/// <summary>
/// Cửa sổ hiển thị cảnh báo người dùng trước khi ứng dụng bắt đầu khóa màn hình học tập.
/// </summary>
public partial class StartupWarningWindow : Window
{
    public StartupWarningWindow(StartupWarningViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.Accepted += () =>
        {
            DialogResult = true;
            Close();
        };

        viewModel.Rejected += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}
