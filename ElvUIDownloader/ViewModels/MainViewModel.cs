using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using ElvUIDownloader.Utils;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ElvUIDownloader.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly RegistryService _registryService;
    public UpdateAddonService UpdateAddonService { get; }
    public UpdateApplicationService UpdateApplicationService { get; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="registryService"></param>
    /// <param name="profileStore"></param>
    /// <param name="addonStore"></param>
    /// <param name="applicationStore"></param>
    /// <param name="updateAddonService"></param>
    /// <param name="updateApplicationService"></param>
    public MainViewModel(AppSettings appSettings, RegistryService registryService,
        ProfileStore profileStore,
        AddonStore addonStore,
        ApplicationStore applicationStore,
        UpdateAddonService updateAddonService,
        UpdateApplicationService updateApplicationService)
    {
        ProfileStore = profileStore;
        AddonStore = addonStore;
        ApplicationStore = applicationStore;

        UpdateApplicationService = updateApplicationService;
        UpdateAddonService = updateAddonService;
        _registryService = registryService;

        UpdateToolTipText = "Обновить";
        UpdateText = "Обновить";

        AppSettings = appSettings;
        var defaultProfile = AppSettings.CurrentProfile;
        ProfileStore.CurrentProfile = AppSettings.Profiles.FirstOrDefault(p => p.Name == defaultProfile);

        IsSelectedProfile = true; // = ProfileStore.CurrentProfile != null;

        Title = $"Профиль - {defaultProfile} - {ApplicationStore.Version}";

        if (AppSettings.StartMinimize) return;
        
        DialogService.UnHide<MainViewModel>();
        IsVisible = true;
    }
    
    [ObservableProperty]
    public partial ProfileStore ProfileStore { get; set; }
    
    [ObservableProperty]
    public partial AddonStore AddonStore { get; set; }
    
    [ObservableProperty]
    public partial ApplicationStore ApplicationStore { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial AppSettings AppSettings { get; set; }

    partial void OnAppSettingsChanged(AppSettings value)
    {
        OnPropertyChanged(nameof(InstallCommand));
        OnPropertyChanged(nameof(UpdateCommand));
        OnPropertyChanged(nameof(DeleteCommand));
    }

    [ObservableProperty]
    public partial bool IsSelectedProfile { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string UpdateToolTipText { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string UpdateText { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanCheckUpdateApplication))]
    private async Task CheckUpdateApplicationAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        await UpdateApplicationService.CheckAsync(cancellationToken);

        var notificationBuilder = new ToastContentBuilder()
               .AddText("Обновление приложения")
               .AddArgument("update_action", "check_update");

        if (ApplicationStore.IsNeedUpdate)
        {
            notificationBuilder
                .AddText("Обновление загружено. Обновить?")

                .AddButton(new ToastButton("Да", "confirm_update"))
                .AddButton(new ToastButtonDismiss("Отмена"));
        }
        else
        {
            notificationBuilder
                .AddText("Обновление не требуется");
        }

        notificationBuilder.Show();

        IsBusy = false;
    }

    private bool CanCheckUpdateApplication()
    {
        return (!IsBusy || IsSelectedProfile) && ProfileStore.CurrentProfile != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanReInstallApplication))]
    private async Task ReInstallApplicationAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        var notificationBuilder = new ToastContentBuilder()
       .AddText("Переустановка приложения")
       .AddArgument("update_action", "check_update")
            .AddText("Переустановить приложение?")

            .AddButton(new ToastButton("Да", "confirm_update"))
            .AddButton(new ToastButtonDismiss("Отмена"));

        notificationBuilder.Show();

        IsBusy = false;
    }

    private bool CanReInstallApplication()
    {
        ApplicationStore.LocalSetupFilename.Refresh();

        return !IsBusy || IsSelectedProfile || ProfileStore.CurrentProfile != null || ApplicationStore.LocalSetupFilename.Exists;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;

            await UpdateAddonService.CheckAsync(cancellationToken);
            await UpdateAddonService.InstallAsync(cancellationToken);

            IsBusy = false;
        }
        catch (Exception)
        {
            IsBusy = false;
        }
    }

    private bool CanInstall()
    {
        var res = !IsBusy && ProfileStore.CurrentProfile != null && AddonStore.Mode == EAddonMode.NeedInstall;

        Debug.WriteLine($"Can Install - {res}");

        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanInstallForce))]
    private async Task InstallForceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;

            await DeleteCommand.ExecuteAsync(null);
            await UpdateAddonService.CheckAsync(cancellationToken);
            await UpdateAddonService.InstallAsync(cancellationToken);
            await UpdateAddonService.CheckAsync(cancellationToken);

            IsBusy = false;
        }
        catch (Exception)
        {
            IsBusy = false;
        }
    }

    private bool CanInstallForce()
    {
        if (IsBusy) return false;
        return ProfileStore.CurrentProfile is { IsForceInstall: true, IsInstalled: true };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanUpdateProfile))]
    private async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;

            await UpdateAddonService.CheckAsync(cancellationToken);
            await UpdateAddonService.InstallAsync(cancellationToken);
            await UpdateAddonService.CheckAsync(cancellationToken);

            IsBusy = false;
        }
        catch (Exception)
        {
            IsBusy = false;
        }
    }

    private bool CanUpdateProfile()
    {
        if (IsBusy) return false;

        if (AddonStore.Mode != EAddonMode.NeedUpdate)
        {
            if (ProfileStore.CurrentProfile is { IsInstalled: true, IsForceUpdate: true })
            {
                return true;
            }

            if (ProfileStore.CurrentProfile is { IsInstalled: true, IsForceUpdate: false })
            {
                return false;
            }

            if (ProfileStore.CurrentProfile is { IsInstalled: false, IsForceUpdate: true })
            {
                return false;
            }
        }

        return ProfileStore.CurrentProfile is { IsForceUpdate: true } || AddonStore.Mode == EAddonMode.NeedUpdate;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;

            if (AddonStore.ElvUi != null)
            {
                foreach (var dir in AddonStore.ElvUi.Directories
                             .Select(elvUiDirectory => Path.Combine(AddonStore.PathToAddons, elvUiDirectory))
                             .Where(Directory.Exists))
                {
                    Directory.Delete(dir, true);
                }

                if (ProfileStore.CurrentProfile != null)
                {
                    ProfileStore.CurrentProfile.Date = null;
                    ProfileStore.CurrentProfile.Version = null;
                    await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile, cancellationToken);
                }

                await UpdateAddonService.CheckAsync(cancellationToken);
            }

            IsBusy = false;
        }
        catch (Exception)
        {
            IsBusy = false;
        }
    }

    private bool CanDelete()
    {
        return !IsBusy && AddonStore is { ElvUi: not null, Mode: EAddonMode.Installed };
    }

    [RelayCommand(CanExecute = nameof(CanDeleteProfile))]
    private async Task DeleteProfileAsync(CancellationToken cancellationToken = default)
    {
        if (ProfileStore.CurrentProfile != null)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name ?? string.Empty);

            var dirInfo = new DirectoryInfo(dir);

            var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{ProfileStore.CurrentProfile.Name}.prof"));

            if (fi.Exists)
            {
                fi.Delete();
            }

            AppSettings.Profiles.Remove(ProfileStore.CurrentProfile);
            ProfileStore.CurrentProfile = AppSettings.Profiles.First();
            AppSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;

            var appDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name!);

            await using (var stream = new FileStream(Path.Combine(appDir, "appsettings.json"), FileMode.Create,
                             FileAccess.Write,
                             FileShare.Write))
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);
            }

            Title = $"Профиль - {AppSettings.CurrentProfile} - {ApplicationStore.Version}";
            await UpdateAddonService.CheckAsync(cancellationToken);
        }
    }

    private bool CanDeleteProfile()
    {
        var res = AppSettings.Profiles.Count > 1;
        return res;
    }

    [RelayCommand]
    private async Task AddProfileAsync(CancellationToken cancellationToken = default)
    {
        await DialogService.ShowDialog<AddProfileViewModel>(async vm =>
        {
            vm.Profile = new ProfileModel
            {
                Type = vm.Types.First()
            };
            vm.InstalledWows = (await _registryService.GetWows()).Select(di => di.FullName).ToList();
            vm.SelectedInstalledWow = vm.InstalledWows.FirstOrDefault();
        }, postClosingAction: async (vm, res) =>
        {

            if (res)
            {
                IsBusy = true;

                AppSettings.Profiles.Add(vm.Profile);
                AppSettings.CurrentProfile = vm.Profile.Name;
                await AppSettings.SaveProfileAsync(vm.Profile, cancellationToken);

                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    name!);

                await using (var stream = new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create,
                                 FileAccess.Write,
                                 FileShare.Write))
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() }
                    };

                    await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);
                }

                ProfileStore.CurrentProfile = vm.Profile;
                Title = $"Профиль - {AppSettings.CurrentProfile} - {ApplicationStore.Version}";
                await UpdateAddonService.CheckAsync(cancellationToken);

                IsBusy = false;
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task EditProfileAsync(CancellationToken cancellationToken = default)
    {
        if (ProfileStore.CurrentProfile != null)
        {
            var oldProfile = new ProfileModel
            {
                Name = ProfileStore.CurrentProfile.Name,
                Type = ProfileStore.CurrentProfile.Type,
                InstallLocation = ProfileStore.CurrentProfile.InstallLocation,
                Version = ProfileStore.CurrentProfile.Version,
                IsForceInstall = ProfileStore.CurrentProfile.IsForceInstall,
                IsForceUpdate = ProfileStore.CurrentProfile.IsForceUpdate,
                Date = ProfileStore.CurrentProfile.Date
            };

            await DialogService.ShowDialog<AddProfileViewModel>(async vm =>
            {
                vm.Title = "Редактировать профиль";
                vm.OkButton = "Изменить";
                vm.Profile = ProfileStore.CurrentProfile;
                vm.InstalledWows = [.. (await _registryService.GetWows()).Select(di => di.FullName)];
                vm.SelectedInstalledWow =
                    vm.InstalledWows.FirstOrDefault(f => f == ProfileStore.CurrentProfile.InstallLocation);
            }, postClosingAction: async (_, res) =>
            {

                if (res)
                {
                    IsBusy = true;

                    var name = Assembly.GetExecutingAssembly().GetName().Name;
                    var dir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);

                    var dirInfo = new DirectoryInfo(dir);

                    var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{AppSettings.CurrentProfile}.prof"));

                    if (fi.Exists)
                    {
                        fi.Delete();
                    }

                    AppSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;
                    await using (var stream = new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create,
                                     FileAccess.Write,
                                     FileShare.Write))
                    {
                        var options = new JsonSerializerOptions
                        {
                            Converters = { new JsonStringEnumConverter() }
                        };

                        await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);
                    }

                    await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile, cancellationToken);

                    Title = $"Профиль - {AppSettings.CurrentProfile} - {ApplicationStore.Version}";

                    await UpdateAddonService.CheckAsync(cancellationToken);

                    IsBusy = false;
                }
                else
                {
                    ProfileStore.CurrentProfile.Name = oldProfile.Name;
                    ProfileStore.CurrentProfile.Type = oldProfile.Type;
                    ProfileStore.CurrentProfile.InstallLocation = oldProfile.InstallLocation;
                    ProfileStore.CurrentProfile.Version = oldProfile.Version;
                    ProfileStore.CurrentProfile.IsForceInstall = oldProfile.IsForceInstall;
                    ProfileStore.CurrentProfile.IsForceUpdate = oldProfile.IsForceUpdate;
                    ProfileStore.CurrentProfile.Date = oldProfile.Date;

                    Title = $"Профиль - {AppSettings.CurrentProfile} - {ApplicationStore.Version}";

                    await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile, cancellationToken);
                }
            });
        }
    }

    private bool CanEditProfile()
    {
        return ProfileStore.CurrentProfile != null;
    }

    [RelayCommand]
    private async Task ProfileSelectedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsBusy = true;

            await UpdateSelectedAsync(cancellationToken);
            await UpdateAddonService.CheckAsync(cancellationToken);

            IsBusy = false;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowViewAsync(CancellationToken cancellationToken = default)
    {
        DialogService.ToggleHide<MainViewModel>();
        IsVisible = !IsVisible;

        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            name!);

        if (!IsVisible)
        {
            AppSettings.StartMinimize = true;
            AddonStore.ElapsedTime.Start();
            ApplicationStore.ElapsedTime.Start();
        }
        else
        {
            AppSettings.StartMinimize = false;
            AddonStore.ElapsedTime.Stop();
            ApplicationStore.ElapsedTime.Stop();
        }

        await using var stream =
            new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write,
                FileShare.Write);

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);

    }

    [RelayCommand]
    private Task ShowNewVersionAsync(CancellationToken cancellationToken = default)
    {
        DialogService.Show<NewVersionViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ClosingAsync(CancellationToken cancellationToken = default)
    {
        DialogService.Hide<MainViewModel>();
        AppSettings.StartMinimize = true;
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            name!);

        await using var stream =
            new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write,
                FileShare.Write);

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);

        IsVisible = false;
        AddonStore.ElapsedTime.Start();
        ApplicationStore.ElapsedTime.Start();
    }

    [RelayCommand]
    private async Task SettingsAsync(CancellationToken cancellationToken = default)
    {
        await DialogService.ShowDialog<SettingsViewModel>(vm =>
        {
            try
            {
                vm.StartMinimized = AppSettings.StartMinimize;
                vm.CurrentProfile = AppSettings.CurrentProfile;

                vm.Theme = AppSettings.Theme;
                vm.UseSystemAccentColor = AppSettings.UseSystemAccentColor;

                if (!string.IsNullOrEmpty(AppSettings.AccentColor))
                {
                    var color = (Color)ColorConverter.ConvertFromString(AppSettings.AccentColor);

                    // Если цвет был добавлен пользовательски
                    var foundNewColor = vm.AccentColors.FirstOrDefault(x => x.Color == color);
                    if (foundNewColor == null)
                    {
                        vm.AccentColors.Add(new SolidColorBrush(color));

                        foundNewColor = vm.AccentColors.FirstOrDefault(x => x.Color == color);
                    }

                    //else
                    //{
                    vm.AccentColor = foundNewColor;
                    //}
                }

                vm.UpdateApplicationSettings.Type = AppSettings.UpdateAppInfo.Type;
                vm.UpdateApplicationSettings.Interval = AppSettings.UpdateAppInfo.Interval;

                vm.UpdateAddonSettings.Type = AppSettings.UpdateAddonInfo.Type;
                vm.UpdateAddonSettings.Interval = AppSettings.UpdateAddonInfo.Interval;
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }, postClosingAction: async (vm, res) =>
        {
            if (res)
            {
                AppSettings.Theme = vm.Theme;
                AppSettings.UseSystemAccentColor = vm.UseSystemAccentColor;
                AppSettings.AccentColor = vm.AccentColor?.Color.ToString();

                AppSettings.UpdateAppInfo.Type = vm.UpdateApplicationSettings.Type;
                AppSettings.UpdateAppInfo.Interval = vm.UpdateApplicationSettings.Interval;

                AppSettings.UpdateAddonInfo.Type = vm.UpdateAddonSettings.Type;
                AppSettings.UpdateAddonInfo.Interval = vm.UpdateAddonSettings.Interval;

                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    name!);
                
                await using var stream = new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write);
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken);
            }
            else
            {
                AccentThemeManager.SetTheme(AppSettings.Theme, vm.UseSystemAccentColor ? null : (Color?)ColorConverter.ConvertFromString(AppSettings.AccentColor));
                
                // App.ApplyTheme(AppSettings);
                // App.ApplyAccentColor(AppSettings.UseSystemAccentColor
                //     ? null
                //     : (Color?)ColorConverter.ConvertFromString(AppSettings.AccentColor));
            }
        });
    }

    [RelayCommand]
    private async Task UpdateSelectedAsync(CancellationToken token = default)
    {
        if (ProfileStore.CurrentProfile != null)
        {
            AppSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;
            await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile, token);

            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name!);

            await using var stream =
                new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write, FileShare.Write);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            await JsonSerializer.SerializeAsync(stream, AppSettings, options, cancellationToken: token);

            Title = $"Профиль - {AppSettings.CurrentProfile} - {ApplicationStore.Version}";
        }
    }

    [RelayCommand]
    private Task ExitAsync(CancellationToken cancellationToken = default)
    {
        Application.Current.Shutdown();
        
        return Task.CompletedTask;
    }
}