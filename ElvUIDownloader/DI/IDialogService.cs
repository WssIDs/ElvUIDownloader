using System.Windows;
using System.Windows.Media;
using ElvUIDownloader.Views.Base;
using Meziantou.Framework.Win32;
using Microsoft.Extensions.DependencyInjection;

namespace ElvUIDownloader.DI;

public class OpenFolderDialogResult
{
    /// <summary>
    /// 
    /// </summary>
    public string? SelectedDirectory { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool Result { get; set; }
}

public class DialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetViewModel<T>() where T : ViewModelBase
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public void Show<T>(Func<T, Task>? preLoadingAction = null, Func<T, Task>? postLoadingAction = null, Func<T, Task>? postClosingAction = null) where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);
        var vm = _serviceProvider.GetRequiredService<T>();

        preLoadingAction?.Invoke(vm);

        var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == name);

        if (type == null) return;
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not IWindow view) return;

        view.SetContext(vm);

        view.OnViewLoaded += View_Loaded;
        view.OnViewClosed += View_Closed;

        view.ShowView();

        void View_Loaded(object? sender, EventArgs args)
        {
            postLoadingAction?.Invoke(vm);
            view.OnViewLoaded -= View_Loaded;
        }

        void View_Closed(object? sender, EventArgs e)
        {
            postClosingAction?.Invoke(vm);
            view.OnViewClosed -= View_Closed;
        }
    }

    public async Task ShowDialog<T>(Func<T, Task>? preLoadingAction = null, Func<T, Task>? postLoadingAction = null, Func<T, bool, Task>? postClosingAction = null) where T : ViewModelBase
    {
        var wnd = Application.Current.Windows.OfType<IWindow>().LastOrDefault(w => w.IsActive);

        //if (wnd == null) return;

        var name = typeof(T).Name.Replace("Model", string.Empty);

        var vm = _serviceProvider.GetRequiredService<T>();

        preLoadingAction?.Invoke(vm);

        var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == name);

        if (type == null) return;
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not IWindow view) return;

        if (wnd != null)
        {
            view.SetOwner(wnd);
        }
        else
        {
            view.SetPosition(WindowStartupLocation.CenterScreen);
        }

        view.SetContext(vm);

        var result = false;

        view.OnViewLoaded += View_Loaded;

        var res = view.ShowView(true);

        result = res == true;

        postClosingAction?.Invoke(vm, result);

        void View_Loaded(object? sender, EventArgs args)
        {
            postLoadingAction?.Invoke(vm);
            view.OnViewLoaded -= View_Loaded;
        }
    }

    public async void ShowHidden<T>(Func<T, Task>? preLoadingAction = null, Func<T, Task>? postLoadingAction = null, Func<T, Task>? postClosingAction = null) where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);
        var vm = _serviceProvider.GetRequiredService<T>();

        preLoadingAction?.Invoke(vm);

        var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == name);

        if (type == null) return;
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not IWindow view) return;
        view.SetContext(vm);

        var loadedTcs = new TaskCompletionSource<bool>();

        view.OnViewLoaded += View_Loaded;
        view.OnViewClosed += View_Closed;

        if (view is not Window wnd) return;

        // 2. Секрет: запоминаем, где окно ДОЛЖНО открыться (например, по центру)
        var originalWindowStartupLocation = wnd.WindowStartupLocation;

        // Временно отключаем центрирование, чтобы принудительно выкинуть окно за экран
        wnd.WindowStartupLocation = WindowStartupLocation.Manual;
        wnd.Left = -32000;
        wnd.Top = -32000;

        // 3. Вызываем Show! Окно открывается в "космосе", пользователь НЕ видит белой вспышки
        view.ShowView();

        // 4. Ждем, пока WPF полностью отрисует окно и все его меню в памяти
        await loadedTcs.Task;

        if (postLoadingAction != null)
        {
            await postLoadingAction.Invoke(vm);
        }

        await Task.Delay(500);

        ////// 5. Окно готово! Возвращаем его на законное место
        if (originalWindowStartupLocation == WindowStartupLocation.CenterScreen)
        {
            // Центрируем окно вручную на правильном мониторе
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            wnd.Left = (screenWidth - wnd.ActualWidth) / 2;
            wnd.Top = (screenHeight - wnd.ActualHeight) / 2;
        }
        else if (originalWindowStartupLocation == WindowStartupLocation.CenterOwner && wnd.Owner != null)
        {
            wnd.Left = wnd.Owner.Left + (wnd.Owner.Width - wnd.ActualWidth) / 2;
            wnd.Top = wnd.Owner.Top + (wnd.Owner.Height - wnd.ActualHeight) / 2;
        }
        else
        {
            // Если координаты были жестко заданы, возвращаем их (например, в 0, 0 или дефолт)
            wnd.Left = 100;
            wnd.Top = 100;
        }

        // Локальные функции
        void View_Loaded(object? sender, EventArgs args)
        {
            view.OnViewLoaded -= View_Loaded;
            loadedTcs.SetResult(true); // Разблокируем await

            if (view is not Window wnd) return;

            wnd.Visibility = Visibility.Hidden;
        }

        void View_Closed(object? sender, EventArgs e)
        {
            view.OnViewClosed -= View_Closed;
            if (postClosingAction != null)
            {
                postClosingAction.Invoke(vm);
            }
        }
    }

    public void ToggleHide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<IWindow>()
            .FirstOrDefault(w => w.GetContext<T>().GetType().Name == typeof(T).Name);

        if (view != null)
        {
            if (view.IsVisible())
            {
                view.Hide();
            }
            else
            {
                view.ShowView();

                if (view.GetState() == WindowState.Minimized)
                {
                    view.SetState(WindowState.Normal);
                }

                view.UpdateLayout();
            }
        }
    }

    public void UnHide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<IWindow>()
               .FirstOrDefault(w => w.GetContext<ViewModelBase>() is T);

 
        if (view == null) return;

        if (!view.IsVisible())
        {
            view.ShowView();
        }

        if (view.GetState() == WindowState.Minimized)
        {
            view.SetState(WindowState.Normal);
        }

        view.UpdateLayout();
    }

    public void Hide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<IWindow>()
            .FirstOrDefault(w => w.GetContext<T>().GetType().Name == typeof(T).Name);

        if (view == null) return;

        if (view.IsVisible())
        {
            view.Hide();
        }
    }

    public OpenFolderDialogResult OpenFolderDialog(string title = "Выберите директорию", string buttonOk = "Выбрать",
        string? initialDirectory = null)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title,
            OkButtonLabel = buttonOk,
            InitialDirectory = initialDirectory
        };

        var res = dialog.ShowDialog();

        var result = new OpenFolderDialogResult();

        if (res == DialogResult.OK)
        {
            result.Result = true;
            result.SelectedDirectory = dialog.SelectedPath;
        }
        else
        {
            result.Result = false;
        }

        return result;
    }
}