using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using English.Repository.Data;
using EnglishManager.Services;

namespace EnglishManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _appDbContext;

    [ObservableProperty]
    private DeckManagementViewModel _deckVM;

    [ObservableProperty]
    private MaterialManagementViewModel _materialVM;

    [ObservableProperty]
    private QuestionManagementViewModel _questionVM;

    [ObservableProperty]
    private InAppDialogService _dialogService;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private int _totalDecks;

    [ObservableProperty]
    private int _totalMaterials;

    [ObservableProperty]
    private int _totalQuestions;

    [ObservableProperty]
    private string _currentDatabasePath = string.Empty;

    [ObservableProperty]
    private string _databaseFileName = string.Empty;

    [ObservableProperty]
    private bool _isDefaultDatabase = true;

    public MainViewModel(
        AppDbContext appDbContext,
        DeckManagementViewModel deckVM,
        MaterialManagementViewModel materialVM,
        QuestionManagementViewModel questionVM,
        InAppDialogService dialogService)
    {
        _appDbContext = appDbContext;
        _deckVM = deckVM;
        _materialVM = materialVM;
        _questionVM = questionVM;
        _dialogService = dialogService;

        UpdateDatabaseInfo(AppDbContext.CurrentDatabasePath);
    }

    private void UpdateDatabaseInfo(string path)
    {
        CurrentDatabasePath = Path.GetFullPath(path);
        DatabaseFileName = Path.GetFileName(path);
        IsDefaultDatabase = string.Equals(DatabaseFileName, "englishlocker.db", StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    public void SelectTab(int index)
    {
        SelectedTabIndex = index;
        _ = RefreshCurrentTabAsync();
    }

    [RelayCommand]
    public void SelectDecksTab() => SelectTab(0);

    [RelayCommand]
    public void SelectMaterialsTab() => SelectTab(1);

    [RelayCommand]
    public void SelectQuestionsTab() => SelectTab(2);

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await DeckVM.LoadDecksCommand.ExecuteAsync(null);
        await MaterialVM.LoadDataCommand.ExecuteAsync(null);
        await QuestionVM.LoadDataCommand.ExecuteAsync(null);
        UpdateCounts();
    }

    private async Task RefreshCurrentTabAsync()
    {
        switch (SelectedTabIndex)
        {
            case 0:
                await DeckVM.LoadDecksCommand.ExecuteAsync(null);
                break;
            case 1:
                await MaterialVM.LoadDataCommand.ExecuteAsync(null);
                break;
            case 2:
                await QuestionVM.LoadDataCommand.ExecuteAsync(null);
                break;
        }
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        TotalDecks = DeckVM.Decks.Count;
        TotalMaterials = MaterialVM.AllMaterials.Count;
        TotalQuestions = QuestionVM.AllQuestions.Count;
    }

    [RelayCommand]
    public async Task SelectDatabaseFileAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Chọn tệp cơ sở dữ liệu SQLite",
            Filter = "SQLite Database (*.db;*.sqlite;*.sqlite3)|*.db;*.sqlite;*.sqlite3|Tất cả tệp (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            await SwitchToDatabaseAsync(dialog.FileName);
        }
    }

    [RelayCommand]
    public async Task CreateDatabaseFileAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Tạo tệp cơ sở dữ liệu SQLite mới",
            Filter = "SQLite Database (*.db)|*.db|Tất cả tệp (*.*)|*.*",
            DefaultExt = ".db",
            FileName = "english_data.db"
        };

        if (dialog.ShowDialog() == true)
        {
            await SwitchToDatabaseAsync(dialog.FileName);
        }
    }

    [RelayCommand]
    public async Task ResetToDefaultDatabaseAsync()
    {
        await SwitchToDatabaseAsync("englishlocker.db");
    }

    private async Task SwitchToDatabaseAsync(string targetPath)
    {
        var previousPath = CurrentDatabasePath;
        try
        {
            _appDbContext.SwitchDatabase(targetPath);
            UpdateDatabaseInfo(targetPath);

            // Nạp lại toàn bộ dữ liệu từ file database mới
            await InitializeAsync();

            await DialogService.ShowSuccessAsync(
                "Chuyển cơ sở dữ liệu thành công",
                $"Đang làm việc và chỉnh sửa trực tiếp trên file:\n{targetPath}\n\nĐã đồng bộ lại danh sách bộ đề, từ mới và câu hỏi.");
        }
        catch (Exception ex)
        {
            try
            {
                _appDbContext.SwitchDatabase(previousPath);
                UpdateDatabaseInfo(previousPath);
            }
            catch { }

            await DialogService.ShowErrorAsync(
                "Lỗi mở cơ sở dữ liệu",
                $"Không thể kết nối hoặc khởi tạo cấu trúc bảng trên file '{Path.GetFileName(targetPath)}':\n{ex.Message}");
        }
    }
}
