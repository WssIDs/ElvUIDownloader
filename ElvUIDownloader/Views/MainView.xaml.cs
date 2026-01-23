using System.ComponentModel;
using ElvUIDownloader.Views.Base;

namespace ElvUIDownloader.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : BaseView
{
    public MainView()
    {
        InitializeComponent();
    }

    private void MainView_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }
}