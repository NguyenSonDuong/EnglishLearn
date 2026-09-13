using System.Windows;
using Application = System.Windows.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using English.Entity.Repositories;
using English.Entity.Services;
using English.Repository.Data;
using English.Repository.Repositories;
using English.Service.Services;
using English.ViewModel.ViewModels;
using EnglishLocker.Views;
using EnglishLocker.Services;

namespace EnglishLocker;

/// <summary>
/// Điểm khởi động ứng dụng EnglishLocker.
///
/// Sử dụng Microsoft.Extensions.DependencyInjection để cấu hình IoC/DI,
/// và ủy quyền mở toàn bộ cửa sổ đa màn hình cho IWindowManagerService.
/// </summary>
public partial class App : Application
{
    /// <summary>DI container toàn cục.</summary>
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Cấu hình và xây dựng DI Container
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // 2. Auto-migrate database (đảm bảo DB sẵn sàng trước khi mở cửa sổ)
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        // 3. Đăng ký ứng dụng làm Custom Shell nếu chưa được cấu hình
        var shellManager = _serviceProvider.GetRequiredService<IShellManagementService>();
        shellManager.CheckAndInstallCustomShell();

        // 4. Khởi tạo toàn bộ cửa sổ đa màn hình thông qua WindowManagerService
        var windowManager = _serviceProvider.GetRequiredService<IWindowManagerService>();
        windowManager.OpenAllWindows();
    }

    /// <summary>
    /// Đăng ký các dependencies vào ServiceCollection.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // ── Database ──
        services.AddDbContext<AppDbContext>();

        // ── Repositories ──
        services.AddScoped<IDeckRepository, DeckRepository>();
        services.AddScoped<ILearningMaterialRepository, LearningMaterialRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IStudyRecordRepository, StudyRecordRepository>();
        services.AddScoped<IStudyHistoryRepository, StudyHistoryRepository>();
        services.AddScoped<IEmergencyLogRepository, EmergencyLogRepository>();
        services.AddScoped<IAppConfigurationRepository, AppConfigurationRepository>();

        // ── Services (Domain) ──
        services.AddScoped<IDeckService, DeckService>();
        services.AddScoped<ILearningMaterialService, LearningMaterialService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IStudyService, StudyService>();
        services.AddScoped<IEmergencyLogService, EmergencyLogService>();
        services.AddScoped<IAppConfigurationService, AppConfigurationService>();

        // ── Services (System) ──
        services.AddSingleton<ISystemControlService, SystemControlService>();
        services.AddSingleton<IHookService, KeyboardHookService>();
        services.AddSingleton<IWindowManagerService, WindowManagerService>();
        services.AddSingleton<IShellManagementService, ShellManagementService>();
        services.AddSingleton<INavigatorService, NavigatorService>();

        // ── ViewModels ──
        services.AddTransient<MainViewModel>();
        services.AddTransient<QuizViewModel>();
        services.AddTransient<StartupWarningViewModel>();

        // ── Views ──
        services.AddTransient<MainWindow>();
        services.AddTransient<BlackoutWindow>();
        services.AddTransient<QuizControl>();
        services.AddTransient<StartupWarningControl>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose DI container để giải phóng tài nguyên hệ thống (bao gồm hook bàn phím)
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
