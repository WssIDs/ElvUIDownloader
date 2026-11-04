using ElvUIDownloader.DI;
using ElvUIDownloader.Hosted;
using ElvUIDownloader.Hosted.Extensions;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using ElvUIDownloader.Utils;
using ElvUIDownloader.ViewModels;
using ElvUIDownloader.Views.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using YandexDisk.API.Client.DI;

#pragma warning disable WPF0001 // ThemeMode помечен как experimental

namespace ElvUIDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly IHost _host;

        public App()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            var builder = Host.CreateDefaultBuilder();

            builder
                .ConfigureAppConfiguration((_, configBuilder) =>
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
                .UseSerilog((_, config) =>
                {
                    var name = Assembly.GetExecutingAssembly().GetName().Name;
                    var dir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var logFilePath = Path.Combine(dir, "logs", "log-.log");

                    config
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug) // Write Debug+ logs to the console
                        .WriteTo.Debug()
                        .WriteTo.File(
                            path: logFilePath,
                            rollingInterval: RollingInterval.Day, // Create a new log file daily
                            restrictedToMinimumLevel: LogEventLevel.Debug, // Write Debug+ logs to the file
                            rollOnFileSizeLimit: true,
                            fileSizeLimitBytes: 10000000, // Limit file size to 10MB
                            retainedFileCountLimit: 7
                        );
                })
                .ConfigureServices((_, services) =>
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
                    services.AddBackgroundService<InstallAddonBackgroundService>();
                });

            _host = builder.Build();
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                SimpleIoC.ServiceProvider = _host.Services;

                var configuration = _host.Services.GetRequiredService<IConfiguration>();

                var appSettings = _host.Services.GetRequiredService<AppSettings>();
                configuration.Bind(appSettings);

                await appSettings.LoadProfilesAsync();

                // Применение темы и цветов

                AccentThemeManager.SetTheme(appSettings.Theme, appSettings.UseSystemAccentColor ? null : (Color?)ColorConverter.ConvertFromString(appSettings.AccentColor));
                
                // ApplyTheme(appSettings);
                // ApplyAccentColor(appSettings.UseSystemAccentColor ? null : (Color?)ColorConverter.ConvertFromString(appSettings.AccentColor));

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

                await _host.RunAsync(token: tokenSource.Token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public static void ApplyTheme(AppSettings appSettings)
        {
            Application.Current.ThemeMode = appSettings.Theme switch
            {
                EApplicationTheme.Light => ThemeMode.Light,
                EApplicationTheme.Dark => ThemeMode.Dark,
                _ => ThemeMode.System   // вместо null
            };
        }

        public static void ApplyAccentColor(Color? baseColor)
        {
            if (baseColor == null)
            {
                Current.Resources.Remove("SystemAccentColor");
                Current.Resources.Remove("AccentButtonBackground");
                Current.Resources.Remove("SystemAccentColorLight1");
                Current.Resources.Remove("AccentButtonBackgroundPointerOver");

                return;
            }

            // Перезаписываем ресурсы на уровне приложения
            Current.Resources["SystemAccentColor"] = baseColor;

            // Желательно программно рассчитать и обновить и кисти:
            Current.Resources["AccentButtonBackground"] = new SolidColorBrush(baseColor.Value);

            // Для оттенков (наведение/клик) можно использовать вспомогательные методы изменения яркости
            Color lightColor = Color.FromArgb(baseColor.Value.A,
                (byte)Math.Min(255, baseColor.Value.R + 30),
                (byte)Math.Min(255, baseColor.Value.G + 30),
                (byte)Math.Min(255, baseColor.Value.B + 30));

            Current.Resources["SystemAccentColorLight1"] = lightColor;
            Current.Resources["AccentButtonBackgroundPointerOver"] = new SolidColorBrush(lightColor);
        }
    }
}