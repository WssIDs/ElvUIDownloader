using System.Diagnostics;
using System.Windows;
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
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not BaseView view) return;
        view.DataContext = vm;

        view.Loaded += View_Loaded;
        view.Closed += View_Closed;
        view.Show();

        void View_Loaded(object sender, RoutedEventArgs args)
        {
            postLoadingAction?.Invoke(vm);
            view.Loaded -= View_Loaded;
        }

        void View_Closed(object? sender, EventArgs e)
        {
            postClosingAction?.Invoke(vm);
            view.Closed -= View_Closed;
        }
    }

    public async Task ShowDialog<T>(Func<T, Task>? preLoadingAction = null, Func<T, Task>? postLoadingAction = null, Func<T, bool, Task>? postClosingAction = null) where T : ViewModelBase
    {
        var wnd = Application.Current.Windows.OfType<BaseView>().LastOrDefault(w => w.IsActive);

        //if (wnd == null) return;

        var name = typeof(T).Name.Replace("Model", string.Empty);

        var vm = _serviceProvider.GetRequiredService<T>();

        preLoadingAction?.Invoke(vm);

        var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == name);

        if (type == null) return;
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not BaseView view) return;

        if (wnd != null)
        {
            view.Owner = wnd;
        }
        else
        {
            view.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        
        view.SetContext(vm);

        var result = false;

        view.Loaded += View_Loaded;
        var res = view.ShowDialog();

        result = res == true;

        postClosingAction?.Invoke(vm, result);

        void View_Loaded(object sender, RoutedEventArgs args)
        {
            postLoadingAction?.Invoke(vm);
            view.Loaded -= View_Loaded;
        }
    }

    public void ShowHidden<T>(Func<T, Task>? preLoadingAction = null, Func<T, Task>? postLoadingAction = null, Func<T, Task>? postClosingAction = null) where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);
        var vm = _serviceProvider.GetRequiredService<T>();

        preLoadingAction?.Invoke(vm);

        var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == name);

        if (type == null) return;
        if (_serviceProvider.GetRequiredKeyedService(type, name) is not BaseView view) return;
        view.SetContext(vm);

        view.Loaded += View_Loaded;
        view.Closed += View_Closed;
        //view.Show();

        void View_Loaded(object sender, RoutedEventArgs args)
        {
            postLoadingAction?.Invoke(vm);
            view.Loaded -= View_Loaded;
        }

        void View_Closed(object? sender, EventArgs e)
        {
            postClosingAction?.Invoke(vm);
            view.Closed -= View_Closed;
        }
    }

    public void ToggleHide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<BaseView>()
            .FirstOrDefault(w => w.DataContext.GetType().Name == typeof(T).Name);

        if (view != null)
        {
            if (view.IsVisible)
            {
                view.Hide();
            }
            else
            {
                view.Show();
                
                if (view.WindowState == WindowState.Minimized)
                {
                    view.WindowState = WindowState.Normal;
                }

                view.UpdateLayout();
            }
        }
    }

    public void UnHide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<BaseView>()
            .FirstOrDefault(w => w.DataContext.GetType().Name == typeof(T).Name);

        if (view is { IsVisible: false })
        {
            view.Show();
        }

        if (view == null) return;
        if (view.WindowState == WindowState.Minimized)
        {
            view.WindowState = WindowState.Normal;
        }

        view.UpdateLayout();
    }

    public void Hide<T>() where T : ViewModelBase
    {
        var name = typeof(T).Name.Replace("Model", string.Empty);

        var view = Application.Current.Windows.OfType<BaseView>()
            .FirstOrDefault(w => w.DataContext.GetType().Name == typeof(T).Name);

        if (view is { IsVisible: true })
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