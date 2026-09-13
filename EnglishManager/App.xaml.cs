using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using English.Entity.Repositories;
using English.Entity.Services;
using English.Repository.Data;
using English.Repository.Repositories;
using English.Service.Services;
using EnglishManager.Services;
using EnglishManager.ViewModels;
using EnglishManager.Views;

namespace EnglishManager;

/// <summary>
/// Điểm khởi động ứng dụng quản lý EnglishManager (Back-office Content Management).
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // 1. Tự động kiểm tra và migrate cơ sở dữ liệu SQLite
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        // 2. Khởi tạo và hiển thị duy nhất 1 cửa sổ MainWindow
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

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
        services.AddScoped<IExcelImportService, ExcelImportService>();

        // ── In-App Dialog Service (Modal overlay trong giao diện) ──
        services.AddSingleton<InAppDialogService>();
        services.AddSingleton<IInAppDialogService>(sp => sp.GetRequiredService<InAppDialogService>());
        services.AddSingleton<INavigatorService, NavigatorService>();

        // ── ViewModels (Quản lý nội bộ trong EnglishManager) ──
        services.AddTransient<DeckManagementViewModel>();
        services.AddTransient<MaterialManagementViewModel>();
        services.AddTransient<QuestionManagementViewModel>();
        services.AddTransient<MainViewModel>();

        // ── Views (Single Window) ──
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
