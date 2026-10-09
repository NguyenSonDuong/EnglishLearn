using System.Windows;
using Application = System.Windows.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using English.Entity.Repositories;
using English.Entity.Services;
using English.Repository.Data;
using English.Repository.Repositories;
using English.Service.Mappings;
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

        // 2. Tự động kiểm tra và khởi tạo cơ sở dữ liệu SQLite
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.EnsureTablesCreated();
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
        // ── Database & Data Access ──
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── AutoMapper ──
        services.AddAutoMapper(typeof(AppMapperProfile));

        // ── Services (System) ──
        services.AddSingleton<ISystemControlService, SystemControlService>();
        services.AddSingleton<IHookService, KeyboardHookService>();
        services.AddSingleton<IWindowManagerService, WindowManagerService>();
        services.AddSingleton<IShellManagementService, ShellManagementService>();
        services.AddSingleton<INavigatorService, NavigatorService>();

        // ── Services (Vocabulary & Domain) ──
        services.AddScoped<IVocabularyService, VocabularyService>();
        services.AddScoped<IVocabularyGeneratorService, DatabaseVocabularyService>();

        // ── ViewModels ──
        services.AddTransient<MainViewModel>();
        services.AddTransient<StartupWarningViewModel>();
        services.AddTransient<LearnWordViewModel>();
        services.AddTransient<LearnWordInformationViewModel>();
        services.AddTransient<LearnWordActionViewModel>();
        services.AddTransient<SelectLevelViewModel>();

        // ── Views ──
        services.AddTransient<MainWindow>();
        services.AddTransient<BlackoutWindow>();
        services.AddTransient<StartupWarningControl>();
        services.AddTransient<LearnWordControl>();
        services.AddTransient<LearnWordInformationControl>();
        services.AddTransient<LearnWordActionControl>();
        services.AddTransient<SelectLevelControl>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose DI container để giải phóng tài nguyên hệ thống (bao gồm hook bàn phím)
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
