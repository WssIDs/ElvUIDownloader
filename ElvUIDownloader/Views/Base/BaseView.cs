using System.Windows;
using MahApps.Metro.Controls;

namespace ElvUIDownloader.Views.Base;

/// <summary>
/// 
/// </summary>
public interface IWindow
{
    void ShowView();

    void SetContext<T>(T context) where T : ViewModelBase;
}

/// <summary>
/// 
/// </summary>
public class BaseView : MetroWindow, IWindow
{
    public void ShowView()
    {
        Show();
    }

    public void SetContext<T>(T context) where T : ViewModelBase
    {
        DataContext = context;
    }
}