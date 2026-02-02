using ElvUIDownloader.Commands;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using ElvUIDownloader.Utils;
using ModernWpf;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ElvUIDownloader.ViewModels;

public class MainViewModel : ViewModelBase
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

        if (!AppSettings.StartMinimize)
        {
            DialogService.UnHide<MainViewModel>();
            IsVisible = true;
        }
    }

    private ProfileStore _profileStore = null!;

    public ProfileStore ProfileStore
    {
        get => _profileStore;
        set => Set(ref _profileStore, value);
    }

    private AddonStore _addonStore = null!;

    public AddonStore AddonStore
    {
        get => _addonStore;
        set => Set(ref _addonStore, value);
    }

    private ApplicationStore _applicationStore = null!;

    public ApplicationStore ApplicationStore
    {
        get => _applicationStore;
        set => Set(ref _applicationStore, value);
    }

    private bool _isBusy;
    private string _updateToolTipText = string.Empty;
    private string _updateText = string.Empty;
    private bool _isSelectedProfile;
    private AppSettings _appSettings = null!;
    private bool _isVisible;
    private bool _isUpdateAppTaskRunning;

    public bool IsUpdateAppTaskRunning
    {
        get => _isUpdateAppTaskRunning;
        set => Set(ref _isUpdateAppTaskRunning, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => Set(ref _isVisible, value);
    }

    public AppSettings AppSettings
    {
        get => _appSettings;
        set
        {
            if (Set(ref _appSettings, value))
            {
                OnPropertyChanged(nameof(InstallCommand));
                OnPropertyChanged(nameof(UpdateCommand));
                OnPropertyChanged(nameof(DeleteCommand));
            }
        }
    }

    public bool IsSelectedProfile
    {
        get => _isSelectedProfile;
        set => Set(ref _isSelectedProfile, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string UpdateToolTipText
    {
        get => _updateToolTipText;
        set => Set(ref _updateToolTipText, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string UpdateText
    {
        get => _updateText;
        set => Set(ref _updateText, value);
    }

    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => Set(ref _isBusy, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand LoadedCommand =>
        new AsyncCommand(async (r) =>
        {
            IsBusy = true;

            await UpdateAddonService.CheckAsync();

            await UpdateApplicationService.CheckAsync();

            if (ApplicationStore.IsNeedUpdate)
            {
                var result = MessageBox.Show($"Обновление загружено. Новая версия - {ApplicationStore.RemoteFileVersionFilename}\nОбновить?", "Обновление приложения",  MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    await UpdateApplicationService.InstallAsync(true);
                }
            }

            IsBusy = false;
        }, (r) => (!IsBusy || IsSelectedProfile) && ProfileStore.CurrentProfile != null);


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand CheckUpdateApplicationCommand =>
        new(async (r) =>
        {
            IsBusy = true;

            await UpdateApplicationService.CheckAsync();

            if (ApplicationStore.IsNeedUpdate)
            {
                var result = MessageBox.Show($"Обновление загружено. Обновить?", "Обновление приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    await UpdateApplicationService.InstallAsync(true);
                }
            }
            else
            {
                MessageBox.Show("Обновление не требуется", "Обновление приложения", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            IsBusy = false;
        }, (r) => (!IsBusy || IsSelectedProfile) && ProfileStore.CurrentProfile != null);


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand ReInstallApplicationCommand =>
        new(async (r) =>
        {
            IsBusy = true;

            var result = MessageBox.Show($"Переустановить приложение?", "Переустановка приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                await UpdateApplicationService.InstallAsync(true);
            }

            IsBusy = false;
        }, (r) => (!IsBusy || IsSelectedProfile) && ProfileStore.CurrentProfile != null);

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand InstallCommand =>
        new(async _ =>
        {
            try
            {
                IsBusy = true;

                await UpdateAddonService.InstallAsync();
                await UpdateAddonService.CheckAsync();

                IsBusy = false;
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        },
            (r) => {

                var res = IsBusy == false && ProfileStore.CurrentProfile != null && AddonStore.Mode == EAddonMode.NeedInstall;

                Debug.WriteLine($"Can Install - {res}");

                return res;
            });


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand InstallForceCommand =>
        new(async _ =>
        {
            try
            {
                IsBusy = true;

                await DeleteCommand.ExecuteAsync();

                await UpdateAddonService.InstallAsync();
                await UpdateAddonService.CheckAsync();

                IsBusy = false;
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }, (r) =>
        {
            if (IsBusy) return false;
            return ProfileStore.CurrentProfile is { IsForceInstall: true, IsInstalled: true };
        });

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand UpdateCommand =>
        new(async _ =>
        {
            try
            {
                IsBusy = true;

                //await UpdateAddonService.InstallAsync();
                await UpdateAddonService.CheckAsync();

                IsBusy = false;
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }, (r) =>
        {
            if (IsBusy) return false;

            if (_addonStore.Mode != EAddonMode.NeedUpdate)
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

            return ProfileStore.CurrentProfile is { IsForceUpdate: true } || _addonStore.Mode == EAddonMode.NeedUpdate;
        });



    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand DeleteCommand =>
        new(async _ =>
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
                        await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile);
                    }

                    await UpdateAddonService.CheckAsync();
                }

                IsBusy = false;
            }
            catch (Exception)
            {
                IsBusy = false;
            }
        }, (r) => 
        IsBusy == false && AddonStore.ElvUi != null && AddonStore.Mode == EAddonMode.Installed);


    public AsyncCommand DeleteProfileCommand =>
        new(async _ =>
        {
            if(ProfileStore.CurrentProfile != null)
            {
                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name ?? string.Empty);

                var dirInfo = new DirectoryInfo(dir);

                var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{ProfileStore.CurrentProfile.Name}.prof"));

                if (fi.Exists)
                {
                    fi.Delete();
                }

                _appSettings.Profiles.Remove(ProfileStore.CurrentProfile);
                ProfileStore.CurrentProfile = _appSettings.Profiles.First();
                _appSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;

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

                    await JsonSerializer.SerializeAsync(stream, _appSettings, options);
                }

                Title = $"Профиль - {_appSettings.CurrentProfile} - {ApplicationStore.Version}";
                await UpdateAddonService.CheckAsync();
            }
        }, CanDeleteProfileCommandExecute);

    private bool CanDeleteProfileCommandExecute(object? param)
    {
        var res = AppSettings.Profiles.Count > 1;
        return res;
    }

    public AsyncCommand AddProfileCommand =>
        new(async _ =>
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
                    _appSettings.CurrentProfile = vm.Profile.Name;
                    await AppSettings.SaveProfileAsync(vm.Profile);

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

                        await JsonSerializer.SerializeAsync(stream, _appSettings, options);
                    }

                    ProfileStore.CurrentProfile = vm.Profile;
                    Title = $"Профиль - {_appSettings.CurrentProfile} - {ApplicationStore.Version}";
                    await UpdateAddonService.CheckAsync();

                    IsBusy = false;
                }
            });
        });

    public AsyncCommand EditProfileCommand =>
        new(async _ =>
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
                    vm.InstalledWows = (await _registryService.GetWows()).Select(di => di.FullName).ToList();
                    vm.SelectedInstalledWow = vm.InstalledWows.FirstOrDefault(f => f == ProfileStore.CurrentProfile.InstallLocation);
                }, postClosingAction: async (vm, res) =>
                {

                    if (res)
                    {
                        IsBusy = true;

                        var name = Assembly.GetExecutingAssembly().GetName().Name;
                        var dir = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            name!);

                        var dirInfo = new DirectoryInfo(dir);

                        var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{_appSettings.CurrentProfile}.prof"));

                        if (fi.Exists)
                        {
                            fi.Delete();
                        }

                        _appSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;
                        await using (var stream = new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create,
                                         FileAccess.Write,
                                         FileShare.Write))
                        {
                            var options = new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() }
                            };

                            await JsonSerializer.SerializeAsync(stream, _appSettings, options);
                        }

                        await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile);

                        Title = $"Профиль - {_appSettings.CurrentProfile} - {ApplicationStore.Version}";

                        await UpdateAddonService.CheckAsync();

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

                        Title = $"Профиль - {_appSettings.CurrentProfile} - {ApplicationStore.Version}";

                        await AppSettings.SaveProfileAsync(ProfileStore.CurrentProfile);
                    }
                });
            }

        }, o => ProfileStore.CurrentProfile != null);

    public AsyncCommand ProfileSelectedCommand =>
        new(async _ =>
        {
            try
            {
                IsBusy = true;

                await UpdateSelectedAsync();
                await UpdateAddonService.CheckAsync();

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

        });

    public AsyncCommand ShowViewCommand =>
        new(async _ =>
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
            await JsonSerializer.SerializeAsync(stream, _appSettings);

        });

    public AsyncCommand ShowNewVersionCommand =>
        new(async _ =>
        {
            DialogService.Show<NewVersionViewModel>();
        });

    public AsyncCommand ClosingCommand =>
        new(async _ =>
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

            await JsonSerializer.SerializeAsync(stream, _appSettings, options);

            IsVisible = false;
            AddonStore.ElapsedTime.Start();
            ApplicationStore.ElapsedTime.Start();
        });

    public AsyncCommand SettingsCommand => new AsyncCommand(async _ =>
    {
        await DialogService.ShowDialog<SettingsViewModel>(async vm =>
        {
            vm.StartMinimized = _appSettings.StartMinimize;
            vm.CurrentProfile = _appSettings.CurrentProfile;

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

                var dirInfo = new DirectoryInfo(dir);

                await using var stream = new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create,
                                 FileAccess.Write,
                                 FileShare.Write);
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                await JsonSerializer.SerializeAsync(stream, AppSettings, options);
            }
            else
            {
                // Возвращаем обратно тему и цвета
                if (AppSettings.Theme == EApplicationTheme.System)
                {
                    ThemeManager.Current.ApplicationTheme = null;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = (ApplicationTheme)AppSettings.Theme;
                }

                if (AppSettings.UseSystemAccentColor)
                {
                    ThemeManager.Current.AccentColor = null;
                }
                else
                {
                    var color = (Color)ColorConverter.ConvertFromString(AppSettings.AccentColor);
                    ThemeManager.Current.AccentColor = color;
                }
            }
        });
    });

    private async Task UpdateSelectedAsync(CancellationToken token = default)
    {
        if (ProfileStore.CurrentProfile != null)
        {
            _appSettings.CurrentProfile = ProfileStore.CurrentProfile.Name;
            await _appSettings.SaveProfileAsync(ProfileStore.CurrentProfile, token);

            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name!);

            await using var stream =
                new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write, FileShare.Write);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            await JsonSerializer.SerializeAsync(stream, _appSettings, options, cancellationToken: token);

            Title = $"Профиль - {_appSettings.CurrentProfile} - {ApplicationStore.Version}";
        }
    }
}