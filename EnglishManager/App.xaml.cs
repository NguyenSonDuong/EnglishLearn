using System.Net.Http;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using English.Entity.Repositories;
using English.Entity.Services;
using English.Repository.Data;
using English.Repository.Repositories;
using English.Service.Mappings;
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

        // 1. Tự động kiểm tra và khởi tạo cơ sở dữ liệu SQLite
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.EnsureTablesCreated();
        }

        // 2. Khởi tạo và hiển thị duy nhất 1 cửa sổ MainWindow
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // ── Database & Unit of Work ──
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── AutoMapper ──
        services.AddAutoMapper(typeof(AppMapperProfile));

        // ── Domain / CRUD Services ──
        services.AddScoped<IVocabularyService, VocabularyService>();
        services.AddScoped<IVocabularyMeaningService, VocabularyMeaningService>();
        services.AddScoped<IMeaningExampleService, MeaningExampleService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();

        // ── AI Dictionary API (HttpClient đăng ký thủ công, không cần NuGet Microsoft.Extensions.Http) ──
        services.AddSingleton<IAiDictionaryApiService>(_ =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8000/"),
                Timeout = TimeSpan.FromSeconds(120)
            };
            return new AiDictionaryApiService(httpClient);
        });

        // ── In-App Dialog Service (Modal overlay trong giao diện) ──
        services.AddSingleton<InAppDialogService>();
        services.AddSingleton<IInAppDialogService>(sp => sp.GetRequiredService<InAppDialogService>());

        // ── ViewModels (Quản lý nội bộ trong EnglishManager) ──
        services.AddTransient<VocabularyManagementViewModel>();
        services.AddTransient<MeaningManagementViewModel>();
        services.AddTransient<ExampleManagementViewModel>();
        services.AddTransient<AiDictionaryViewModel>();
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
