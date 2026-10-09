using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Entity.DTOs;
using English.Entity.Services;

namespace English.ViewModel.ViewModels;

/// <summary>
/// ViewModel quản lý trang học và luyện gõ từ mới.
/// Điều phối giữa hai thành phần: Information Panel (thông tin từ) và Action Panel (Action Tab động).
/// Kế thừa PageViewModelBase để đồng bộ điều hướng SPA trong MainWindow.
/// </summary>
public partial class LearnWordViewModel : PageViewModelBase
{
    // ──────────────────────────── Dependencies ──────────────────────────────

    private readonly INavigatorService _navigator;
    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;
    private readonly LearnWordInformationViewModel _informationViewModel;
    private readonly IServiceProvider _serviceProvider;

    // ──────────────────────────── Observable Properties ─────────────────────

    /// <summary>ViewModel hiển thị thông tin chi tiết từ vựng (Information Panel).</summary>
    public LearnWordInformationViewModel InformationViewModel => _informationViewModel;

    /// <summary>ViewModel của Action Tab hiện đang được kích hoạt và hiển thị.</summary>
    [ObservableProperty]
    private object? _actionViewModel;

    // ──────────────────────────── Constructor ───────────────────────────────

    public LearnWordViewModel(
        INavigatorService navigator,
        IVocabularyGeneratorService vocabularyGeneratorService,
        LearnWordInformationViewModel informationViewModel,
        IServiceProvider serviceProvider)
    {
        _navigator = navigator;
        _vocabularyGeneratorService = vocabularyGeneratorService;
        _informationViewModel = informationViewModel;
        _serviceProvider = serviceProvider;

        PageTitle = "Học Từ Mới";

        _informationViewModel.VocabularyChanged += OnVocabularyChanged;

        // Khởi tạo tab Action mặc định ban đầu
        SwitchActionTab<LearnWordActionViewModel>();

        _ = InitializeVocabularyAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    #region Tab Switching / Quản lý Chuyển đổi Action Tab
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Chuyển đổi Action Tab sang một ViewModel khác chỉ bằng việc truyền class ViewModel.
    /// Tab cũ sẽ bị hủy liên kết, vô hiệu hóa, giải phóng và dọn dẹp sạch sẽ như thể chưa từng tồn tại.
    /// </summary>
    /// <typeparam name="TViewModel">Class của ViewModel Tab cần chuyển sang.</typeparam>
    /// <param name="configure">Delegate tùy chọn để cấu hình ViewModel mới trước khi hiển thị.</param>
    /// <returns>Instance của ViewModel mới được khởi tạo và kích hoạt.</returns>
    public TViewModel SwitchActionTab<TViewModel>(Action<TViewModel>? configure = null) where TViewModel : class
    {
        // 1. Dọn dẹp, disable và giải phóng sạch sẽ tab cũ
        ClearCurrentActionTab();

        // 2. Khởi tạo instance mới từ DI Container hoặc Activator
        TViewModel newTabInstance = ResolveTabViewModel<TViewModel>();

        // 3. Cấu hình bổ sung nếu có
        configure?.Invoke(newTabInstance);

        // 4. Kết nối logic và sự kiện cần thiết cho tab mới
        AttachActionTab(newTabInstance);

        // 5. Cập nhật thuộc tính hiển thị lên giao diện
        ActionViewModel = newTabInstance;

        return newTabInstance;
    }

    /// <summary>
    /// Chuyển đổi Action Tab theo Type (Non-generic overload).
    /// </summary>
    /// <param name="viewModelType">Type của ViewModel Tab cần chuyển sang.</param>
    /// <param name="configure">Delegate cấu hình bổ sung.</param>
    /// <returns>Instance của ViewModel mới được kích hoạt.</returns>
    public object SwitchActionTab(Type viewModelType, Action<object>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        ClearCurrentActionTab();

        object newTabInstance = ResolveTabViewModel(viewModelType);

        configure?.Invoke(newTabInstance);

        AttachActionTab(newTabInstance);

        ActionViewModel = newTabInstance;

        return newTabInstance;
    }

    /// <summary>
    /// Vô hiệu hóa, hủy toàn bộ event subscription và dọn dẹp sạch sẽ tab cũ như thể nó chưa từng tồn tại.
    /// </summary>
    private void ClearCurrentActionTab()
    {
        if (ActionViewModel == null) return;

        // Gỡ bỏ sự kiện nếu tab cũ là LearnWordActionViewModel
        if (ActionViewModel is LearnWordActionViewModel oldActionVm)
        {
            oldActionVm.WordVisibilityChanged -= OnWordVisibilityChanged;
            oldActionVm.WordCompleted -= OnWordCompletedAsync;
        }

        // Gọi Dispose nếu tab cũ hỗ trợ dọn dẹp tài nguyên
        if (ActionViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // Xóa hoàn toàn tham chiếu
        ActionViewModel = null;
    }

    /// <summary>
    /// Phân giải ViewModel từ DI Container hoặc Activator.
    /// </summary>
    private TViewModel ResolveTabViewModel<TViewModel>() where TViewModel : class
    {
        return (TViewModel)ResolveTabViewModel(typeof(TViewModel));
    }

    private object ResolveTabViewModel(Type viewModelType)
    {
        object? instance = _serviceProvider.GetService(viewModelType);
        if (instance != null) return instance;

        return Activator.CreateInstance(viewModelType)
            ?? throw new InvalidOperationException($"Không thể khởi tạo tab ViewModel '{viewModelType.FullName}'.");
    }

    /// <summary>
    /// Gắn các liên kết sự kiện và truyền dữ liệu cần thiết cho Action Tab mới.
    /// </summary>
    private void AttachActionTab(object tabInstance)
    {
        if (tabInstance is LearnWordActionViewModel actionVm)
        {
            actionVm.WordVisibilityChanged += OnWordVisibilityChanged;
            actionVm.WordCompleted += OnWordCompletedAsync;

            // Đồng bộ từ hiện tại nếu InformationViewModel đã có dữ liệu
            if (!string.IsNullOrEmpty(_informationViewModel.WordText))
            {
                actionVm.SetupWord(_informationViewModel.WordText);
            }
        }
    }

    #endregion

    // ──────────────────────────── Commands ──────────────────────────────────

    [RelayCommand]
    private async Task Loaded()
    {
        try
        {
            await LoadedAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    // ──────────────────────────── Private Methods ───────────────────────────

    private async Task LoadedAsync()
    {
        try
        {
            await InitializeVocabularyAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Nạp từ vựng ban đầu từ cơ sở dữ liệu SQLite và truyền cho các Sub-ViewModels.
    /// </summary>
    private async Task InitializeVocabularyAsync()
    {
        try
        {
            var dto = await _vocabularyGeneratorService.GenerateVocabularyAsync(_informationViewModel.Level);
            if (dto != null)
            {
                _informationViewModel.LoadVocabularyDto(dto);
                if (ActionViewModel is LearnWordActionViewModel actionVm)
                {
                    actionVm.SetupWord(dto.WordText);
                }
            }
        }
        catch (Exception)
        {
            // Bỏ qua lỗi kết nối / DB fallback
        }
    }

    /// <summary>
    /// Đồng bộ khi InformationViewModel nạp hoặc đổi từ mới.
    /// </summary>
    private void OnVocabularyChanged(VocabularyDto dto)
    {
        if (ActionViewModel is LearnWordActionViewModel actionVm)
        {
            actionVm.SetupWord(dto.WordText);
        }
    }

    /// <summary>
    /// Đồng bộ ẩn/hiện thông tin chi tiết từ khi Action Tab yêu cầu.
    /// </summary>
    private void OnWordVisibilityChanged(bool isVisible)
    {
        _informationViewModel.IsWordVisible = isVisible;
    }

    /// <summary>
    /// Tự động nạp từ mới khi người dùng hoàn thành số lượt gõ yêu cầu.
    /// </summary>
    private async Task OnWordCompletedAsync()
    {
        await InitializeVocabularyAsync();
    }
}
