# HƯỚNG DẪN VÀ QUY TẮC PHÁT TRIỂN DỰ ÁN ENGLISHLOCKER (GEMINI.md)

Tài liệu này định nghĩa toàn bộ quy tắc kiến trúc, quy chuẩn mã nguồn và hướng dẫn thực thi bắt buộc cho Gemini / AI Assistant khi duy trì và phát triển dự án **EnglishLocker**.

---

## 1. TỔNG QUAN HỆ THỐNG VÀ TECH STACK

- **Mục đích**: Ứng dụng khóa màn hình học tiếng Anh (Kiosk-mode), chống gian lận, hỗ trợ đa màn hình vật lý, tích hợp custom Windows Shell (thay thế `explorer.exe`).
- **Nền tảng**: .NET 8.0 Windows (`net8.0-windows`).
- **Giao diện**: WPF (Windows Presentation Foundation) & Windows Forms (hỗ trợ `Screen.AllScreens` đa màn hình).
- **Cơ sở dữ liệu**: SQLite qua Entity Framework Core 8.x.
- **Mô hình MVVM**: `CommunityToolkit.Mvvm` (8.x).
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection` (8.x).
- **Hệ thống Win32 APIs**: User32 hooks (`SetWindowsHookEx`), Registry HKCU (`Winlogon\Shell`, `DisableTaskMgr`), Process management.

---

## 2. QUY TẮC KIẾN TRÚC PHÂN TẦNG (BẮT BUỘC TUÂN THỦ)

Giải pháp tuân theo kiến trúc phân tầng độc lập (**Clean / Onion Architecture**), lấy `English.Entity` làm **trung tâm tuyệt đối**.

### Sơ đồ phụ thuộc (Dependency Graph)
```
                       ┌─────────────────────────┐
                       │     English.Entity      │  <── TRUNG TÂM TUYỆT ĐỐI
                       │ (DTOs, Entities, Enums, │      (Không reference bất kỳ project nào)
                       │  IRepository, IService, │
                       │         Helpers)        │
                       └────────────▲────────────┘
                                    │
         ┌──────────────────────────┼──────────────────────────┐
         │                          │                          │
┌────────┴──────────┐      ┌────────┴─────────┐       ┌────────┴─────────┐
│English.Repository │      │ English.Service  │       │English.ViewModel │
│   (AppDbContext,  │      │(Service Impls,   │       │ (MainViewModel,  │
│  Repo Impls,      │      │ SysControl,      │       │     Commands)    │
│   Migrations)     │      │ ShellManagement, │       │                  │
│                   │      │ KeyboardHook)    │       │                  │
└────────▲──────────┘      └────────▲─────────┘       └────────▲─────────┘
         │          ▲               │          ▲               │
         │          │               │          │               │
         │          └───────┬───────┘          └───────┬───────┘
         │                  │                          │
         │                  │                          │ references all 4 modules
         │     ┌────────────┴────────────┐┌────────────┴────────────┐
         │     │     EnglishManager      ││      EnglishLocker      │
         └────►│(Quản lý từ, câu hỏi,    ││ (Views, DI Container,   │
               │ bộ đề - Single Window,  ││  WindowManagerService,  │
               │ tự chứa ViewModels riêng││     App.xaml / .cs)     │
               └─────────────────────────┘└─────────────────────────┘
```

---

## 3. TRÁCH NHIỆM VÀ PHÂN VÙNG CỦA TỪNG MODULE

### 3.1. `English.Entity` (Core / Center)
- **Quy tắc vàng**: **KHÔNG ĐƯỢC PHÉP reference tới bất kỳ project nào trong solution**.
- **Chứa**:
  - `Entities/`: Toàn bộ các thực thể cơ sở dữ liệu (`Question`, `Deck`, `LearningMaterial`, `StudyRecord`, v.v.).
  - `DTOs/`: Toàn bộ Data Transfer Objects trao đổi giữa các tầng (`QuestionDto`, `DeckDto`, v.v.).
  - `Enums/`: Các Enum nghiệp vụ (`CategoryType`, `TestType`).
  - `Repositories/`: Toàn bộ Interface Repository (`IRepository<T>`, `IQuestionRepository`, v.v.).
  - `Services/`: Toàn bộ Interface Service nghiệp vụ và hệ thống (`IQuestionService`, `IHookService`, `ISystemControlService`, `IShellManagementService`, `IWindowManagerService`).
  - `Helpers/`: Các class tiện ích thuần túy (serialization, mapping, calculation) phục vụ chung cho toàn app.

### 3.2. `English.Repository` (Data Access Layer)
- **Dependencies**: Chỉ reference `English.Entity` + thư viện EF Core (SQLite, Design, Tools).
- **Chứa**:
  - `Data/`: `AppDbContext` kế thừa từ `DbContext`.
  - `Migrations/`: Lịch sử migration cơ sở dữ liệu EF Core.
  - `Repositories/`: Các class triển khai của Repository interfaces (`Repository<T>`, `QuestionRepository`, v.v.).

### 3.3. `English.Service` (Business & System Service Layer)
- **Dependencies**: Chỉ reference `English.Entity`.
- **Nguyên lý Dependency Inversion**:
  - Service chỉ phụ thuộc vào Repository Interface (`English.Entity.Repositories.I*Repository`), **KHÔNG ĐƯỢC reference trực tiếp sang `English.Repository`**.
- **Chứa**:
  - Các service nghiệp vụ câu hỏi, học tập (`QuestionService`, `DeckService`, `StudyService`, v.v.).
  - Các service hệ thống Windows không gắn trực tiếp vào View (`KeyboardHookService`, `ShellManagementService`, `SystemControlService`).

### 3.4. `English.ViewModel` (Presentation Logic Layer)
- **Dependencies**: Chỉ reference `English.Entity` + `CommunityToolkit.Mvvm`.
- **Quy tắc**:
  - Không phụ thuộc vào `English.Service` hay `English.Repository`. Chỉ giao tiếp qua các Service Interface thuộc `English.Entity.Services`.
  - Không phụ thuộc trực tiếp vào các lớp View WPF cụ thể (`Window`, `UserControl`).
- **Chứa**:
  - `ViewModels/`: `MainViewModel` kế thừa `ObservableObject`, quản lý state câu hỏi, anti-cheat attempts, relay commands.

### 3.5. `EnglishLocker` (Main WPF Executable & View Layer)
- **Dependencies**: Reference tới cả 4 project: `English.Entity`, `English.Repository`, `English.Service`, `English.ViewModel`.
- **Vai trò giới hạn**: **Chỉ làm nơi cấu hình DI và khởi tạo View**.
- **Chứa**:
  - `App.xaml` và `App.xaml.cs`: Đăng ký Dependency Injection (`AddScoped`, `AddSingleton`, `AddTransient`), chạy Auto-migration DB, kích hoạt Windows Hook / Shell.
  - `Views/`: `MainWindow.xaml` (màn hình chính), `BlackoutWindow.xaml` (màn hình phụ che đen).
  - `Converters/`: Các Value Converter WPF (`CheatCountToVisibilityConverter`).
  - `Services/`: `WindowManagerService` – triển khai interface `IWindowManagerService` để điều khiển hiển thị đa màn hình vật lý và đóng mở View.

### 3.6. `EnglishManager` (Standalone WPF Management Application)
- **Mục đích**: Ứng dụng quản trị/nội dung độc lập (Content & Deck Manager) phục vụ quản lý từ mới (Learning Materials), câu hỏi (Questions) và bộ đề (Decks) trên cơ sở dữ liệu SQLite.
- **Dependencies**: Chỉ reference 3 project: `English.Entity`, `English.Repository`, và `English.Service`.
- **Quy tắc vàng của EnglishManager**:
  - **TUYỆT ĐỐI KHÔNG reference** tới `EnglishLocker` hay `English.ViewModel`.
  - **Tự quản lý ViewModel**: Toàn bộ ViewModels phục vụ quản lý phải nằm trực tiếp trong project `EnglishManager` (namespace `EnglishManager.ViewModels`), không được đặt trong `English.ViewModel`.
  - **Single-Window & In-App Dialog**: Ứng dụng chỉ hoạt động trong 1 Window duy nhất. Toàn bộ thông báo, xác nhận, xóa, cảnh báo hoặc form chi tiết phải hiển thị dạng Dialog/Modal Overlay bên trong giao diện (In-App Dialog Overlay), **tuyệt đối không mở MessageBox/MessageDialog của hệ thống Windows**.

---

## 4. QUY CHUẨN ĐẶT TÊN VÀ NAMESPACES

Tuân thủ chuẩn namespace tương ứng với từng module:
```csharp
// English.Entity
namespace English.Entity.Entities;
namespace English.Entity.DTOs;
namespace English.Entity.Enums;
namespace English.Entity.Repositories;
namespace English.Entity.Services;
namespace English.Entity.Helpers;

// English.Repository
namespace English.Repository.Data;
namespace English.Repository.Repositories;
namespace English.Repository.Migrations;

// English.Service
namespace English.Service.Services;

// English.ViewModel
namespace English.ViewModel.ViewModels;

// EnglishLocker
namespace EnglishLocker;
namespace EnglishLocker.Views;
namespace EnglishLocker.Converters;
namespace EnglishLocker.Services;

// EnglishManager
namespace EnglishManager;
namespace EnglishManager.Views;
namespace EnglishManager.ViewModels;
namespace EnglishManager.Converters;
namespace EnglishManager.Services;
```

---

## 5. QUY TẮC PHÁT TRIỂN TÍNH NĂNG MỚI (STEP-BY-STEP WORKFLOW)

Khi được yêu cầu bổ sung một tính năng hoặc sửa đổi luồng nghiệp vụ, Gemini phải tuân thủ đúng thứ tự 5 bước:

1. **Bước 1 (`English.Entity`)**: Khai báo/cập nhật Entity, DTO, Enum, Helper và các Interface mới (Repository / Service).
2. **Bước 2 (`English.Repository`)**: Cập nhật `AppDbContext` (nếu có bảng mới), tạo Migration và triển khai Repository class.
3. **Bước 3 (`English.Service`)**: Triển khai logic nghiệp vụ trong Service class tương ứng thông qua Interface từ `English.Entity`.
4. **Bước 4 (`English.ViewModel`)**: Khai báo dependency trong ViewModel, quản lý thuộc tính observable (`[ObservableProperty]`) và hành động (`[RelayCommand]`).
5. **Bước 5 (`EnglishLocker`)**:
   - Đăng ký DI trong `App.xaml.cs` (phân loại chính xác: `AddScoped` cho DB/Domain Service, `AddSingleton` cho Service hệ thống, `AddTransient` cho ViewModel và View).
   - Thiết kế hoặc cập nhật XAML View nếu có tương tác UI.

---

## 6. QUY TẮC AN TOÀN VÀ XÁC MINH (VERIFICATION)

- **Tránh Circular Dependency**: Không bao giờ thêm reference từ `English.Entity`, `English.Repository`, `English.Service`, `English.ViewModel` ngược về `EnglishLocker`.
- **Clean Build**: Sau bất kỳ thay đổi code nào, luôn thực hiện lệnh build để kiểm tra:
  ```powershell
  dotnet build EnglishLocker.sln
  ```
- **Tiêu chuẩn chất lượng**: **0 Warning(s), 0 Error(s)** trước khi thông báo hoàn thành nhiệm vụ cho người dùng.
- **Giữ nguyên định dạng Solution**: Mọi project mới (nếu có) phải được đồng bộ đăng ký vào cả hai file `EnglishLocker.sln` (chuẩn) và `EnglishLocker.slnx` (XML).
