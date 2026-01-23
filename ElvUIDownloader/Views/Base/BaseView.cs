using System.Windows;

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
    public event EventHandler? OnViewLoaded;

    public event EventHandler? OnViewClosed;

    public BaseView()
    {
        Loaded += BaseView_Loaded;
        Closed += BaseView_Closed;
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