using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using English.Entity.Enums;
using UserControl = System.Windows.Controls.UserControl;

namespace EnglishLocker.Controls;

/// <summary>
/// UserControl điều phối hiển thị và tạo hiệu ứng đóng/mở/chuyển trang (Page Transition Animation)
/// mượt mà giữa các UserControl trong kiến trúc Single Page.
/// 
/// Cơ chế hoạt động:
/// 1. Khi có trang mới, chụp ảnh tức thì (VisualBrush snapshot) của trang hiện tại gán vào PreviousContainer.
/// 2. Nạp ViewModel của trang mới vào CurrentPresenter.
/// 3. Thực thi Storyboard chuyển động đồng thời:
///    - Trang mới: Fade-In (Opacity 0 -> 1) kết hợp trượt theo trục X (theo chiều Forward / Backward).
///    - Trang cũ: Fade-Out (Opacity 1 -> 0) kết hợp trượt nhẹ để nhường chỗ.
/// 4. Sử dụng hàm gia tốc CubicEase (EaseOut) giúp chuyển động tự nhiên, không giật lag.
/// </summary>
public partial class PageTransitionControl : UserControl
{
    private Storyboard? _currentStoryboard;

    // ──────────────────────────── Dependency Properties ─────────────────────

    public static readonly DependencyProperty PageContentProperty =
        DependencyProperty.Register(
            nameof(PageContent),
            typeof(object),
            typeof(PageTransitionControl),
            new PropertyMetadata(null, OnPageContentChanged));

    public static readonly DependencyProperty TransitionDirectionProperty =
        DependencyProperty.Register(
            nameof(TransitionDirection),
            typeof(NavigationTransitionDirection),
            typeof(PageTransitionControl),
            new PropertyMetadata(NavigationTransitionDirection.Forward));

    /// <summary>
    /// Nội dung hoặc ViewModel của Page đang hoạt động.
    /// </summary>
    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    /// <summary>
    /// Hướng di chuyển animation (Forward / Backward / None).
    /// </summary>
    public NavigationTransitionDirection TransitionDirection
    {
        get => (NavigationTransitionDirection)GetValue(TransitionDirectionProperty);
        set => SetValue(TransitionDirectionProperty, value);
    }

    public PageTransitionControl()
    {
        InitializeComponent();
    }

    private static void OnPageContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageTransitionControl control)
        {
            control.HandleContentTransition(e.OldValue, e.NewValue);
        }
    }

    /// <summary>
    /// Xử lý hiệu ứng chuyển đổi giữa view cũ và view mới.
    /// </summary>
    private void HandleContentTransition(object? oldContent, object? newContent)
    {
        // Dừng animation cũ nếu đang chạy dở
        if (_currentStoryboard != null)
        {
            _currentStoryboard.Stop();
            _currentStoryboard = null;
        }

        if (newContent == null)
        {
            CurrentPresenter.Content = null;
            PreviousContainer.Visibility = Visibility.Collapsed;
            PreviousContainer.Background = null;
            return;
        }

        // Trường hợp 1: Khởi động lần đầu (chưa có oldContent hoặc chưa render kích thước)
        if (oldContent == null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            CurrentPresenter.Content = newContent;
            PreviousContainer.Visibility = Visibility.Collapsed;
            PreviousContainer.Background = null;
            AnimateEntrance();
            return;
        }

        // Trường hợp 2: Chuyển đổi giữa 2 Page (đã có oldContent trên màn hình)
        AnimatePageSwitch(newContent);
    }

    /// <summary>
    /// Hiệu ứng xuất hiện trang ban đầu khi mở ứng dụng (Fade-In + Slide-Up nhẹ).
    /// </summary>
    private void AnimateEntrance()
    {
        CurrentPresenter.Opacity = 0;
        CurrentTranslate.X = 0;
        CurrentTranslate.Y = 25;

        var duration = TimeSpan.FromMilliseconds(320);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var opacityAnim = new DoubleAnimation(0, 1, duration) { EasingFunction = ease };
        var slideAnim = new DoubleAnimation(25, 0, duration) { EasingFunction = ease };

        var sb = new Storyboard();
        sb.Children.Add(opacityAnim);
        sb.Children.Add(slideAnim);

        Storyboard.SetTarget(opacityAnim, CurrentPresenter);
        Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(OpacityProperty));

        Storyboard.SetTarget(slideAnim, CurrentTranslate);
        Storyboard.SetTargetProperty(slideAnim, new PropertyPath(TranslateTransform.YProperty));

        _currentStoryboard = sb;
        sb.Begin();
    }

    /// <summary>
    /// Hiệu ứng chuyển động qua lại giữa 2 Page (Snapshot + Slide X + Fade).
    /// </summary>
    private void AnimatePageSwitch(object newContent)
    {
        // 1. Chụp nhanh Visual snapshot của view cũ gán làm background cho PreviousContainer
        try
        {
            var brush = new VisualBrush(CurrentPresenter)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            PreviousContainer.Background = brush;
            PreviousContainer.Visibility = Visibility.Visible;
            PreviousContainer.Opacity = 1;
            PreviousTranslate.X = 0;
            PreviousTranslate.Y = 0;
        }
        catch
        {
            PreviousContainer.Visibility = Visibility.Collapsed;
        }

        // 2. Nạp content mới vào CurrentPresenter
        CurrentPresenter.Content = newContent;
        CurrentPresenter.Opacity = 0;
        CurrentTranslate.Y = 0;

        // 3. Tính toán hướng trượt (Forward hay Backward)
        double currentStartX;
        double previousEndX;

        switch (TransitionDirection)
        {
            case NavigationTransitionDirection.Backward:
                currentStartX = -60; // Trang mới từ bên trái bay vào
                previousEndX = 60;   // Trang cũ trượt sang phải
                break;

            case NavigationTransitionDirection.None:
                currentStartX = 0;
                previousEndX = 0;
                break;

            case NavigationTransitionDirection.Forward:
            default:
                currentStartX = 60;  // Trang mới từ bên phải bay vào
                previousEndX = -60;  // Trang cũ trượt sang trái
                break;
        }

        CurrentTranslate.X = currentStartX;

        // 4. Tạo Storyboard chuyển động đồng thời
        var duration = TimeSpan.FromMilliseconds(280);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var newOpacity = new DoubleAnimation(0, 1, duration) { EasingFunction = ease };
        var newSlide = new DoubleAnimation(currentStartX, 0, duration) { EasingFunction = ease };

        var oldOpacity = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(240)) { EasingFunction = ease };
        var oldSlide = new DoubleAnimation(0, previousEndX, duration) { EasingFunction = ease };

        var sb = new Storyboard();
        sb.Children.Add(newOpacity);
        sb.Children.Add(newSlide);
        sb.Children.Add(oldOpacity);
        sb.Children.Add(oldSlide);

        Storyboard.SetTarget(newOpacity, CurrentPresenter);
        Storyboard.SetTargetProperty(newOpacity, new PropertyPath(OpacityProperty));

        Storyboard.SetTarget(newSlide, CurrentTranslate);
        Storyboard.SetTargetProperty(newSlide, new PropertyPath(TranslateTransform.XProperty));

        Storyboard.SetTarget(oldOpacity, PreviousContainer);
        Storyboard.SetTargetProperty(oldOpacity, new PropertyPath(OpacityProperty));

        Storyboard.SetTarget(oldSlide, PreviousTranslate);
        Storyboard.SetTargetProperty(oldSlide, new PropertyPath(TranslateTransform.XProperty));

        sb.Completed += (s, e) =>
        {
            PreviousContainer.Visibility = Visibility.Collapsed;
            PreviousContainer.Background = null;
            _currentStoryboard = null;
        };

        _currentStoryboard = sb;
        sb.Begin();
    }
}
