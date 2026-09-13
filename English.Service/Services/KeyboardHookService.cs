using English.Entity.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace English.Service.Services;

/// <summary>
/// Low-Level Keyboard Hook sử dụng Win32 API để chặn các tổ hợp phím "thoát":
///   • Alt+Tab, Alt+Esc        → chuyển cửa sổ
///   • Ctrl+Esc, phím Windows  → mở Start Menu
/// 
/// Cơ chế hoạt động:
///   1. SetWindowsHookEx(WH_KEYBOARD_LL, callback, ...) đăng ký hook toàn cục.
///   2. Mỗi lần phím được nhấn/nhả, Windows gọi callback của ta.
///   3. Ta kiểm tra vKey + modifier → nếu là tổ hợp cấm, trả về 1 (nuốt phím).
///   4. Nếu không cấm, gọi CallNextHookEx để chuyển tiếp cho app khác.
/// </summary>
public class KeyboardHookService : IHookService
{
    // ──────────────────────────── Win32 Constants ────────────────────────────

    /// <summary>Loại hook: Low-Level Keyboard (mã 13).</summary>
    private const int WH_KEYBOARD_LL = 13;

    /// <summary>Thông điệp khi phím được nhấn xuống.</summary>
    private const int WM_KEYDOWN = 0x0100;

    /// <summary>Thông điệp khi phím hệ thống được nhấn (ví dụ: Alt+Key).</summary>
    private const int WM_SYSKEYDOWN = 0x0104;

    // Virtual Key codes
    private const int VK_TAB      = 0x09;
    private const int VK_SHIFT    = 0x10;
    private const int VK_ESCAPE   = 0x1B;
    private const int VK_LWIN     = 0x5B;  // phím Windows trái
    private const int VK_RWIN     = 0x5C;  // phím Windows phải
    private const int VK_LSHIFT   = 0xA0;  // phím Shift trái
    private const int VK_RSHIFT   = 0xA1;  // phím Shift phải

    // ──────────────────────────── Win32 Imports ─────────────────────────────

    /// <summary>
    /// Đăng ký một hook procedure vào chuỗi hook của Windows.
    /// </summary>
    /// <param name="idHook">Loại hook (WH_KEYBOARD_LL = 13).</param>
    /// <param name="lpfn">Con trỏ tới hàm callback.</param>
    /// <param name="hMod">Handle của module chứa hook (dùng module hiện tại).</param>
    /// <param name="dwThreadId">0 = hook toàn cục (tất cả thread).</param>
    /// <returns>Handle của hook, dùng để gỡ bỏ sau này.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    /// <summary>Gỡ bỏ hook đã cài đặt.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    /// <summary>
    /// Chuyển tiếp sự kiện phím cho hook tiếp theo trong chuỗi.
    /// Nếu ta "nuốt" phím (return 1), ta KHÔNG gọi hàm này.
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>Lấy handle của module (DLL/EXE) theo tên.</summary>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>Kiểm tra trạng thái của một phím (bit cao = đang nhấn).</summary>
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    // ──────────────────────────── Delegate & Fields ─────────────────────────

    /// <summary>Delegate cho hàm callback của Low-Level Keyboard Hook.</summary>
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>Handle trả về từ SetWindowsHookEx, cần lưu để gỡ hook.</summary>
    private IntPtr _hookId = IntPtr.Zero;

    /// <summary>
    /// GIỮ THAM CHIẾU tới delegate để GC không thu hồi.
    /// Nếu không giữ, callback bị GC → crash khi Windows gọi lại.
    /// </summary>
    private readonly LowLevelKeyboardProc _proc;

    // ──────────────────────────── Constructor ───────────────────────────────

    public KeyboardHookService()
    {
        // Lưu delegate vào field để tránh bị Garbage Collected
        _proc = HookCallback;
    }

    // ──────────────────────────── Public Methods ────────────────────────────

    /// <summary>
    /// Cài đặt Low-Level Keyboard Hook toàn cục.
    /// </summary>
    public void InstallHook()
    {
        if (_hookId != IntPtr.Zero) return; // đã cài rồi

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;

        // Đăng ký hook với module handle của process hiện tại, threadId = 0 (toàn cục)
        _hookId = SetWindowsHookEx(
            WH_KEYBOARD_LL,
            _proc,
            GetModuleHandle(curModule.ModuleName!),
            0);

        Debug.WriteLine($"[KeyboardHook] Hook installed. Handle = {_hookId}");
    }

    /// <summary>
    /// Gỡ bỏ hook, trả lại quyền điều khiển phím cho hệ thống.
    /// </summary>
    public void UninstallHook()
    {
        if (_hookId == IntPtr.Zero) return; // chưa cài

        UnhookWindowsHookEx(_hookId);
        Debug.WriteLine($"[KeyboardHook] Hook uninstalled. Handle = {_hookId}");
        _hookId = IntPtr.Zero;
    }

    // ──────────────────────────── Core Callback ─────────────────────────────

    /// <summary>
    /// Hàm callback được Windows gọi mỗi khi có sự kiện bàn phím.
    /// </summary>
    /// <param name="nCode">Nếu >= 0, ta được phép xử lý.</param>
    /// <param name="wParam">Loại thông điệp (WM_KEYDOWN, WM_SYSKEYDOWN, ...).</param>
    /// <param name="lParam">Con trỏ tới struct KBDLLHOOKSTRUCT chứa vkCode.</param>
    /// <returns>
    ///   (IntPtr)1  → "nuốt" phím, không cho hệ thống xử lý.
    ///   CallNextHookEx → chuyển tiếp bình thường.
    /// </returns>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;

            // Chỉ xử lý khi phím được nhấn (không xử lý nhả phím)
            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                // Đọc virtual key code từ struct KBDLLHOOKSTRUCT
                // (vkCode nằm ở offset 0, là int đầu tiên)
                int vkCode = Marshal.ReadInt32(lParam);

                // Kiểm tra trạng thái các phím modifier
                bool altPressed   = IsKeyDown(0xA4) || IsKeyDown(0xA5); // VK_LMENU / VK_RMENU
                bool ctrlPressed  = IsKeyDown(0xA2) || IsKeyDown(0xA3); // VK_LCONTROL / VK_RCONTROL
                bool shiftPressed = IsKeyDown(VK_LSHIFT) || IsKeyDown(VK_RSHIFT) || IsKeyDown(VK_SHIFT);

                // ── Chặn Alt+Tab ──
                if (altPressed && vkCode == VK_TAB)
                {
                    Debug.WriteLine("[KeyboardHook] Blocked: Alt+Tab");
                    return (IntPtr)1;
                }

                // ── Chặn Alt+Esc ──
                if (altPressed && vkCode == VK_ESCAPE)
                {
                    Debug.WriteLine("[KeyboardHook] Blocked: Alt+Esc");
                    return (IntPtr)1;
                }

                // ── Phím tắt mở Task Manager: Ctrl + Shift + Esc ──
                if (ctrlPressed && shiftPressed && vkCode == VK_ESCAPE)
                {
                    if (IsDebugMode())
                    {
                        Debug.WriteLine("[KeyboardHook] Debug mode: Cho phép phím tắt mở Task Manager (Ctrl+Shift+Esc)");
                        return CallNextHookEx(_hookId, nCode, wParam, lParam);
                    }

                    Debug.WriteLine("[KeyboardHook] Release mode: Chặn phím tắt mở Task Manager (Ctrl+Shift+Esc)");
                    return (IntPtr)1;
                }

                // ── Chặn Ctrl+Esc (mở Start Menu) khi không kèm Shift ──
                if (ctrlPressed && !shiftPressed && vkCode == VK_ESCAPE)
                {
                    Debug.WriteLine("[KeyboardHook] Blocked: Ctrl+Esc");
                    return (IntPtr)1;
                }

                // ── Chặn phím Windows (Left / Right) ──
                if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                {
                    Debug.WriteLine("[KeyboardHook] Blocked: Windows Key");
                    return (IntPtr)1;
                }
            }
        }

        // Phím hợp lệ → chuyển tiếp cho hook/app tiếp theo
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    // ──────────────────────────── Helper ─────────────────────────────────────

    /// <summary>
    /// Kiểm tra ứng dụng có đang chạy ở chế độ Debug hay không.
    /// Trả về true nếu build với định nghĩa DEBUG hoặc đang có Debugger gắn vào (attach).
    /// </summary>
    private static bool IsDebugMode()
    {
#if DEBUG
        return true;
#else
        return Debugger.IsAttached;
#endif
    }

    /// <summary>
    /// Kiểm tra phím có đang được nhấn hay không.
    /// GetAsyncKeyState trả về short, bit cao (0x8000) = đang nhấn.
    /// </summary>
    private static bool IsKeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    // ──────────────────────────── IDisposable ───────────────────────────────

    public void Dispose()
    {
        UninstallHook();
        GC.SuppressFinalize(this);
    }
}
