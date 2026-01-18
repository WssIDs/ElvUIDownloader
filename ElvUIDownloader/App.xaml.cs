using ElvUIDownloader.DI;
using ElvUIDownloader.Hosted;
using ElvUIDownloader.Hosted.Extensions;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using ElvUIDownloader.Utils;
using ElvUIDownloader.ViewModels;
using ElvUIDownloader.Views;
using ElvUIDownloader.Views.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModernWpf;
using Serilog;
using Serilog.Events;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using YandexDisk.API.Client.DI;

namespace ElvUIDownloader
{
    // public class SimpleIoC
    // {
    //     private static readonly SimpleIoC instance = new SimpleIoC();
    //
    //     public ServiceProvider Provider { get; private set; }
    //     
    //     public ServiceCollection Services { get; private set; }
    //
    //     private SimpleIoC()
    //     {
    //         Services = new ServiceCollection();
    //     }
    //
    //     public static SimpleIoC GetInstance()
    //     {
    //         return instance;
    //     }
    //
    //     public void Build()
    //     {
    //         Provider = Services.BuildServiceProvider();
    //     }
    // }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            var builder = Host.CreateDefaultBuilder();

            builder
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    const string applicationSettingsFileName = "appsettings.json";

                    var name = Assembly.GetExecutingAssembly().GetName().Name;

                    var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);

                    if (File.Exists(Path.Combine(dir, applicationSettingsFileName)))
                    {
                        configBuilder
                            .SetBasePath(dir)
                            .AddJsonFile(applicationSettingsFileName, optional: false, reloadOnChange: true);
                    }
                    else
                    {
                        configBuilder
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(applicationSettingsFileName, optional: false, reloadOnChange: true);
                    }
                })
                .UseSerilog((context, config) =>
                {
                    var name = Assembly.GetExecutingAssembly().GetName().Name;
                    var dir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    string logFilePath = Path.Combine(dir, "logs", "log-.log");

                    config
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug) // Write Debug+ logs to the console
                        .WriteTo.Debug()
                        .WriteTo.File(
                            path: logFilePath,
                            rollingInterval: RollingInterval.Day, // Create a new log file daily
                            restrictedToMinimumLevel: LogEventLevel.Debug, // Write Debug+ logs to the file
                            rollOnFileSizeLimit: true,
                            fileSizeLimitBytes: 10000000, // Limit file size to 10MB
                            retainedFileCountLimit : 7
                        );
                })
                .ConfigureServices((context, services) =>
                {
                    // ...

                    services.AddHttpClient("TukuiClient", client =>
                    {
                        client.BaseAddress = new Uri("https://api.tukui.org/v1/");
                    });

                    services.AddYandexDiskApi(options =>
                    {
                        options.AccessToken = "y0_AgAAAAAYlmlrAArc2QAAAADyqLtfppXHAPaNQySw1s5gN0ZovfovCq8";
                    });

                    // Config
                    services.AddSingleton<AppSettings>();
                    
                    // Dialog
                    services.AddSingleton<DialogService>();
                    // Registry
                    services.AddSingleton<RegistryService>();

                    // Stores
                    services.AddSingleton<ProfileStore>();
                    services.AddSingleton<AddonStore>();
                    services.AddSingleton<ApplicationStore>();

                    // Update Services
                    services.AddSingleton<UpdateAddonService>();
                    services.AddSingleton<UpdateApplicationService>();

                    var viewModels = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.BaseType == typeof(ViewModelBase));

                    foreach (var viewModel in viewModels)
                    {
                        services.AddTransient(viewModel);
                    }

                    var views = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(BaseView));

                    foreach (var view in views)
                    {
                        services.AddKeyedTransient(view, view.Name);
                    }

                    // Background
                    services.AddBackgroundService<UpdateAddonBackgroundService>();
                    services.AddBackgroundService<UpdateApplicationBackgroundService>();
                    services.AddBackgroundService<InstallApplicationBackgroundService>();
                });

            _host = builder.Build();
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            SimpleIoC.ServiceProvider = _host.Services;
            
            var configuration = _host.Services.GetRequiredService<IConfiguration>();
            
            var appSettings = _host.Services.GetRequiredService<AppSettings>();
            configuration.Bind(appSettings);
            
            await appSettings.LoadProfilesAsync();

            // Применение темы и цветов

            if(appSettings.Theme == EApplicationTheme.System)
            {
                ThemeManager.Current.ApplicationTheme = null;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = (ApplicationTheme)appSettings.Theme;
            }

            if (appSettings.UseSystemAccentColor)
            {
                ThemeManager.Current.AccentColor = null;
            }
            else
            {
                var color = (Color)ColorConverter.ConvertFromString(appSettings.AccentColor);
                ThemeManager.Current.AccentColor = color;
            }

            var tokenSource = new CancellationTokenSource();

            var updateApplicationService = _host.Services.GetRequiredService<UpdateApplicationService>();
            await updateApplicationService.CheckAsync(tokenSource.Token);

            var dialogService = _host.Services.GetRequiredService<DialogService>();

            if (appSettings.StartMinimize)
            {
                dialogService.ShowHidden<MainViewModel>(async vm =>
                {
                    Debug.WriteLine("Show Hidden WND");

                    if (vm.AppSettings.StartMinimize)
                    {
                        dialogService.Hide<MainViewModel>();
                        vm.IsVisible = false;
                        try
                        {
                            vm.AddonStore.ElapsedTime.Start();
                            vm.ApplicationStore.ElapsedTime.Start();

                            //var appTask = vm.UpdateApplicationService.CheckAsync(tokenSource.Token);
                            var addonTask = vm.UpdateAddonService.CheckAsync(tokenSource.Token);

                            await Task.WhenAll(addonTask);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Задача отменена - {ex}");
                        }
                        finally
                        {
                            tokenSource.Dispose();
                        }
                    }
                });
            }
            else
            {
                dialogService.Show<MainViewModel>(async vm =>
                {
                    var appTask = vm.UpdateApplicationService.CheckAsync(tokenSource.Token);
                    var addonTask = vm.UpdateAddonService.CheckAsync(tokenSource.Token);

                    await Task.WhenAll(appTask, addonTask);

                    Debug.WriteLine("Show WND");
                });
            }
            
            await _host.RunAsync();
        }
    }
}