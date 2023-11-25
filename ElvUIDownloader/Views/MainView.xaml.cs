using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using ElvUIDownloader.Models.Web;
using ElvUIDownloader.Views.Base;
using Microsoft.Win32;

namespace ElvUIDownloader.Views
{
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
}