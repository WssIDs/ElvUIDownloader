using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ElvUIDownloader.Views.Base;



/// <summary>
/// 
/// </summary>
public interface IWindow
{
    // Определяем событие
    public event EventHandler? OnViewLoaded;

    // Определяем событие
    public event EventHandler? OnViewClosed;

    event EventHandler? OnSourceViewInitialized;

    bool? ShowView(bool dilaog = false);

    void SetContext<T>(T context) where T : ViewModelBase;

    T GetContext<T>() where T : ViewModelBase;

    void SetOwner<T>(T owner) where T : IWindow;

    void SetPosition(WindowStartupLocation startupLocation);

    void UpdateLayout();

    bool IsVisible();

    WindowState GetState();

    void SetState(WindowState state);

    void Hide();

    bool IsActive { get; }
}

/// <summary>
/// 
/// </summary>
public class BaseView : Window, IWindow
{
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    // Стили Win32
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_SYSMENU = 0x00080000;

    private const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    public event EventHandler? OnViewLoaded;

    public event EventHandler? OnViewClosed;

    public event EventHandler? OnSourceViewInitialized;

    public BaseView()
    {
        Loaded += BaseView_Loaded;
        Closed += BaseView_Closed;
        SourceInitialized += BaseView_SourceInitialized;
    }

    private void BaseView_SourceInitialized(object? sender, EventArgs e)
    {
        OnSourceViewInitialized?.Invoke(null, EventArgs.Empty);
        SourceInitialized -= BaseView_SourceInitialized;
    }

    private void BaseView_Closed(object? sender, EventArgs e)
    {
        OnViewClosed?.Invoke(null, EventArgs.Empty);
        Closed -= BaseView_Closed;
    }

    private void BaseView_Loaded(object sender, RoutedEventArgs e)
    {
        OnViewLoaded?.Invoke(null, EventArgs.Empty);
        Loaded -= BaseView_Loaded;
    }

    public bool? ShowView(bool dialog)
    {
        if (dialog)
        {
            return ShowDialog();
        }

        Show();
      
        return null;
    }

    public void SetContext<T>(T context) where T : ViewModelBase
    {
        DataContext = context;
    }

    public void SetOwner<T>(T owner) where T : IWindow
    {
        Owner = owner as BaseView;
    }

    public void SetPosition(WindowStartupLocation startupLocation)
    {
        WindowStartupLocation = startupLocation;
    }

    public T GetContext<T>() where T : ViewModelBase
    {
        return (T)DataContext;
    }

    bool IWindow.IsVisible()
    {
        return IsVisible;
    }

    public void SetState(WindowState state)
    {
        WindowState = state;
    }

    public WindowState GetState()
    {
        return WindowState;
    }

    bool IWindow.IsActive => IsActive;
}