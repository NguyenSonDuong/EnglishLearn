using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel quản lý chức năng học và luyện gõ từ mới.
/// Kế thừa PageViewModelBase để đồng bộ điều hướng SPA trong MainWindow.
/// </summary>
public partial class LearnWordViewModel : PageViewModelBase
{
    // ──────────────────────────── Observable Properties ─────────────────────

    /// <summary>Từ tiếng Anh gốc cần học.</summary>
    [ObservableProperty]
    private string _wordText = string.Empty;

    /// <summary>Phiên âm IPA của từ.</summary>
    [ObservableProperty]
    private string _phonetic = string.Empty;

    /// <summary>Danh sách các nghĩa và ví dụ tương ứng của từ.</summary>
    [ObservableProperty]
    private ObservableCollection<MeaningMockModel> _meanings = new();

    /// <summary>Nội dung người dùng đang gõ trong ô nhập liệu.</summary>
    [ObservableProperty]
    private string _userInput = string.Empty;

    /// <summary>Lần gõ chính xác hiện tại.</summary>
    [ObservableProperty]
    private int _currentAttempt;

    /// <summary>Tổng số lần cần gõ hoàn thành (mặc định 10).</summary>
    [ObservableProperty]
    private int _targetAttempts = 10;

    /// <summary>Cờ điều khiển việc ẩn/hiện từ gốc ở khu vực luyện gõ.</summary>
    [ObservableProperty]
    private bool _isWordVisible = true;

    /// <summary>Thông báo trạng thái phản hồi cho người dùng.</summary>
    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    /// <summary>Mã màu hex cho thông báo phản hồi (xanh khi đúng, cam/đỏ khi cảnh báo/sai).</summary>
    [ObservableProperty]
    private string _feedbackColor = "#6C63FF";

    // ──────────────────────────── Constructor ───────────────────────────────

    public LearnWordViewModel()
    {
        PageTitle = "Học Từ Mới";
        InitMockData();
    }

    // ──────────────────────────── Commands ──────────────────────────────────

    /// <summary>
    /// Command xử lý khi người dùng nhấn Enter hoặc ấn nút Xác nhận.
    /// Giữ rỗng theo yêu cầu để tích hợp logic nghiệp vụ sau.
    /// </summary>
    [RelayCommand]
    private void Submit()
    {
        // Để trống theo yêu cầu kiến trúc
    }

    // ──────────────────────────── Mock Data ─────────────────────────────────

    private void InitMockData()
    {
        WordText = "resilient";
        Phonetic = "/rɪˈzɪliənt/";
        CurrentAttempt = 3;
        TargetAttempts = 10;
        IsWordVisible = true;
        UserInput = string.Empty;
        FeedbackMessage = "💡 Hãy nhìn kỹ từ và gõ lại thật chính xác vào ô bên dưới.";
        FeedbackColor = "#6C63FF";

        Meanings = new ObservableCollection<MeaningMockModel>
        {
            new MeaningMockModel
            {
                WordClass = "ADJECTIVE",
                DefinitionVI = "Có khả năng phục hồi nhanh chóng sau khó khăn, kiên cường, bền bỉ.",
                Examples = new List<string>
                {
                    "She is remarkably resilient in the face of difficulties.",
                    "The local economy has proven to be remarkably resilient."
                }
            },
            new MeaningMockModel
            {
                WordClass = "VERB",
                DefinitionVI = "Bật lại, co giãn, phục hồi hình dạng ban đầu sau khi bị nén ép.",
                Examples = new List<string>
                {
                    "Rubber is a resilient material that quickly returns to its original form."
                }
            }
        };
    }
}
