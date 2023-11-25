using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using ElvUIDownloader.Commands;
using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Web;
using ElvUIDownloader.Services;
using ElvUIDownloader.Utils;

namespace ElvUIDownloader.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly RegistryService _registryService;
    private readonly UpdateService _updateService;
    private string _title;
    private ElvUiModel? _elvUi;
    private bool _isNeedUpdate;
    private bool _isNeedInstall;

    private CancellationTokenSource _token;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="registryService"></param>
    /// <param name="updateService"></param>
    public MainViewModel(AppSettings appSettings, RegistryService registryService, UpdateService updateService)
    {
        _registryService = registryService;
        _updateService = updateService;
        
        UpdateToolTipText = "Обновить";
        UpdateText = "Обновить";

        _token = new CancellationTokenSource();

        AppSettings = appSettings;
        var defaultProfile = AppSettings.CurrentProfile;
        CurrentProfile = AppSettings.Profiles.First(p => p.Name == defaultProfile);

        IsSelectedProfile = true;// = CurrentProfile != null;

        var executingAssembly = Assembly.GetExecutingAssembly();
        AppVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).ProductVersion ?? string.Empty;

        Title = $"Профиль - {defaultProfile} - {AppVersion}";

        if (AppSettings.StartMinimize)
        {
            DialogService.Hide<MainViewModel>();
            IsVisible = false;
            CheckAppUpdate(_token.Token).FireAndForgetSafeAsync();
            CheckAddonUpdate(_token.Token).FireAndForgetSafeAsync();
        }
        else
        {
            DialogService.UnHide<MainViewModel>();
            IsVisible = true;
        }
    }

    private ProfileModel? _currentProfile;
    private string _pathToAddons;
    private bool _isBusy;
    private float _progress;
    private bool _isDownloading;
    private bool _isInstalling;
    private string _updateToolTipText;
    private string _updateText;
    private string _textVersionInfo;
    private bool _isSelectedProfile;
    private AppSettings _appSettings;
    private string _appVersion;
    private bool _isVisible;


    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public string AppVersion
    {
        get => _appVersion;
        set => SetField(ref _appVersion, value);
    }

    public AppSettings AppSettings
    {
        get => _appSettings;
        set
        {
            if (SetField(ref _appSettings, value))
            {
                OnPropertyChanged(nameof(InstallCommand));
                OnPropertyChanged(nameof(UpdateCommand));
                OnPropertyChanged(nameof(DeleteCommand));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TextVersionInfo
    {
        get => _textVersionInfo;
        set => SetField(ref _textVersionInfo, value);
    }

    public bool IsSelectedProfile
    {
        get => _isSelectedProfile;
        set => SetField(ref _isSelectedProfile, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string UpdateToolTipText
    {
        get => _updateToolTipText;
        set => SetField(ref _updateToolTipText, value);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public string UpdateText
    {
        get => _updateText;
        set => SetField(ref _updateText, value);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public ProfileModel? CurrentProfile
    {
        get => _currentProfile;
        set
        {
            if (!SetField(ref _currentProfile, value)) return;
            if (_currentProfile != null)
            {
                _appSettings.CurrentProfile = _currentProfile.Name;
            }
        }
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    /// <summary>
    /// /
    /// </summary>
    public ElvUiModel? ElvUi
    {
        get => _elvUi;
        set => SetField(ref _elvUi, value);
    }

    public bool IsNeedUpdate
    {
        get => _isNeedUpdate;
        set => SetField(ref _isNeedUpdate, value);
    }

    public bool IsNeedInstall
    {
        get => _isNeedInstall;
        set => SetField(ref _isNeedInstall, value);
    }

    public string PathToAddons
    {
        get => _pathToAddons;
        set => SetField(ref _pathToAddons, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set =>SetField(ref _isBusy, value);
    }

    public float Progress
    {
        get => _progress;
        set => SetField(ref _progress, value);
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set => SetField(ref _isDownloading, value);
    }

    public bool IsInstalling
    {
        get => _isInstalling;
        set => SetField(ref _isInstalling, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand LoadedCommand =>
        new AsyncCommand(async (r) =>
        {
            IsBusy = true;
            await CheckAddonAsync();
            IsBusy = false;
        }, (r) => !IsBusy || IsSelectedProfile);


    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand InstallCommand =>
        new AsyncCommand(async (r) =>
        {
            try
            {
                IsBusy = true;

                await InstallAddonAsync();

                IsBusy = false;
            }
            catch (Exception exception)
            {
                IsBusy = false;
            }
        }, (r) => IsBusy == false );
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand InstallForceCommand =>
        new AsyncCommand(async (r) =>
        {
            try
            {
                IsBusy = true;

                await InstallAddonAsync();

                IsBusy = false;
            }
            catch (Exception exception)
            {
                IsBusy = false;
            }
        }, (r) =>
        {
            if (IsBusy) return false;
            return CurrentProfile is { IsForceInstall: true };
        });
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand UpdateCommand =>
        new AsyncCommand(async (r) =>
        {
            try
            {
                IsBusy = true;

                await UpdateAddonAsync();

                IsBusy = false;
            }
            catch (Exception exception)
            {
                IsBusy = false;
            }
        }, (r) =>
        {
            if (IsBusy) return false;

            if (!IsNeedUpdate)
            {
                if (CurrentProfile is { IsInstalled: true, IsForceUpdate: true })
                {
                    return true;
                }

                if (CurrentProfile is { IsInstalled: true, IsForceUpdate: false })
                {
                    return false;
                }

                if(CurrentProfile is { IsInstalled: false, IsForceUpdate: true })
                {
                    return false;
                }
            }

            return CurrentProfile is { IsForceUpdate: true } || IsNeedUpdate;
        });
    
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand DeleteCommand =>
        new AsyncCommand(async (r) =>
        {
            try
            {
                IsBusy = true;
                
                if (ElvUi != null)
                {
                    foreach (var dir in ElvUi.directories
                                 .Select(elvUiDirectory => Path.Combine(PathToAddons, elvUiDirectory))
                                 .Where(Directory.Exists))
                    {
                        Directory.Delete(dir, true);
                    }

                    if (CurrentProfile != null)
                    {
                        CurrentProfile.Date = null;
                        CurrentProfile.Version = null;
                        await AppSettings.SaveProfileAsync(CurrentProfile);
                    }

                    await CheckAddonAsync();
                }

                IsBusy = false;
            }
            catch (Exception exception)
            {
                IsBusy = false;
            }
        }, (r) => IsBusy == false );

    
    public AsyncCommand DeleteProfileCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            {
                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),name);

                var dirInfo = new DirectoryInfo(dir);
                    
                var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{CurrentProfile.Name}.prof"));

                if (fi.Exists)
                {
                    fi.Delete();
                }

                _appSettings.Profiles.Remove(CurrentProfile);
                CurrentProfile = _appSettings.Profiles.First();
                _appSettings.CurrentProfile = CurrentProfile.Name;

                await using (var stream = new FileStream("appsettings.json", FileMode.Create, FileAccess.Write,
                                 FileShare.Write))
                {
                    await JsonSerializer.SerializeAsync(stream, _appSettings.Profiles);
                }
                
                Title = $"Профиль - {_appSettings.CurrentProfile} - {AppVersion}";
                await CheckAddonAsync();
            }
        }, CanDeleteProfileCommandExecute);
    }

    private bool CanDeleteProfileCommandExecute(object? param)
    {
        var res = AppSettings.Profiles.Count > 1;
        return res;
    }

    public AsyncCommand AddProfileCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            var vm = DialogService.GetViewModel<AddProfileViewModel>();
            vm.Profile = new ProfileModel();
            vm.Profile.Type = vm.Types.First();
            vm.InstalledWows = (await _registryService.GetWows()).Select(di => di.FullName).ToList();
            vm.SelectedInstalledWow = vm.InstalledWows.FirstOrDefault();
            
            await DialogHost.Show(vm, "addProfile", (sender, args) =>
            {
                Console.WriteLine(args.Source);
            }, (sender, args) =>
            {
                Console.WriteLine(args);
            }, async (sender, args) =>
            {
                bool? res = args.Parameter == null ? null : Convert.ToBoolean(args.Parameter);
                
                if (res == true)
                {
                    IsBusy = true;
                    
                    AppSettings.Profiles.Add(vm.Profile);
                    _appSettings.CurrentProfile = vm.Profile.Name;
                    await AppSettings.SaveProfileAsync(vm.Profile);
                   
                    var name = Assembly.GetExecutingAssembly().GetName().Name;
                    var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);
                    
                    await using (var stream = new FileStream(Path.Combine(dir,"appsettings.json"), FileMode.Create, FileAccess.Write,
                                     FileShare.Write))
                    {
                        await JsonSerializer.SerializeAsync(stream, _appSettings);
                    }
                    
                    CurrentProfile = vm.Profile;
                    Title = $"Профиль - {_appSettings.CurrentProfile} - {AppVersion}";
                    await CheckAddonAsync();

                    IsBusy = false;
                }
                Console.WriteLine(args.Parameter);
            });
        });
    }
    
    public AsyncCommand EditProfileCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            var vm = DialogService.GetViewModel<AddProfileViewModel>();

            var oldProfile = new ProfileModel
            {
                Name = CurrentProfile.Name,
                Type = CurrentProfile.Type,
                InstallLocation = CurrentProfile.InstallLocation,
                Version = CurrentProfile.Version,
                IsForceInstall = CurrentProfile.IsForceInstall,
                IsForceUpdate = CurrentProfile.IsForceUpdate,
                Date = CurrentProfile.Date
            };
            
            vm.Title = "Редактировать профиль";
            vm.OkButton = "Изменить";
            vm.Profile = CurrentProfile;
            vm.InstalledWows = (await _registryService.GetWows()).Select(di => di.FullName).ToList();
            vm.SelectedInstalledWow = vm.InstalledWows.FirstOrDefault(f => f == CurrentProfile.InstallLocation);
            
            await DialogHost.Show(vm, "addProfile", (sender, args) =>
            {
                Console.WriteLine(args.Source);
            }, (sender, args) =>
            {
                Console.WriteLine(args);
            }, async (sender, args) =>
            {
                var res = Convert.ToBoolean(args.Parameter);
                
                if (res)
                {
                    IsBusy = true;

                    var name = Assembly.GetExecutingAssembly().GetName().Name;
                    var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        name!);

                    var dirInfo = new DirectoryInfo(dir);
                    
                    var fi = new FileInfo(Path.Combine(dirInfo.FullName, $"{_appSettings.CurrentProfile}.prof"));

                    if (fi.Exists)
                    {
                        fi.Delete();
                    }

                    _appSettings.CurrentProfile = CurrentProfile.Name;
                    await using (var stream = new FileStream(Path.Combine(dir,"appsettings.json"), FileMode.Create, FileAccess.Write,
                                     FileShare.Write))
                    {
                        await JsonSerializer.SerializeAsync(stream, _appSettings);
                    }

                    await AppSettings.SaveProfileAsync(CurrentProfile);
                    
                    Title = $"Профиль - {_appSettings.CurrentProfile} - {AppVersion}";
                    
                    await CheckAddonAsync();
                    
                    IsBusy = false;
                }
                else
                {
                    CurrentProfile.Name = oldProfile.Name;
                    CurrentProfile.Type = oldProfile.Type;
                    CurrentProfile.InstallLocation = oldProfile.InstallLocation;
                    CurrentProfile.Version = oldProfile.Version;
                    CurrentProfile.IsForceInstall = oldProfile.IsForceInstall;
                    CurrentProfile.IsForceUpdate = oldProfile.IsForceUpdate;
                    CurrentProfile.Date = oldProfile.Date;
                    
                    Title = $"Профиль - {_appSettings.CurrentProfile} - {AppVersion}";
                    
                    await AppSettings.SaveProfileAsync(CurrentProfile);
                }
                Console.WriteLine(args.Parameter);
            });
        });
    }

    public AsyncCommand ProfileSelectedCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            try
            {
                IsBusy = true;
                await UpdateSelectedAsync();
                await CheckAddonAsync();
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
    }

    public AsyncCommand ShowViewCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            DialogService.ToggleHide<MainViewModel>();
            IsVisible = !IsVisible;

            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name!);
            
            if (!IsVisible)
            {
                AppSettings.StartMinimize = true;
                
                _token = new CancellationTokenSource();
                CheckAppUpdate(_token.Token).FireAndForgetSafeAsync();
                CheckAddonUpdate(_token.Token).FireAndForgetSafeAsync();
            }
            else
            {
                AppSettings.StartMinimize = false;
                _token.Cancel();
            }
            
            await using var stream =
                new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write, FileShare.Write);
            await JsonSerializer.SerializeAsync(stream, _appSettings);
            
        });
    }

    public AsyncCommand ClosingCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            DialogService.Hide<MainViewModel>();
            AppSettings.StartMinimize = true;
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                name!);
                
            await using var stream =
                new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write, FileShare.Write);
            await JsonSerializer.SerializeAsync(stream, _appSettings);
            
            IsVisible = false;

            _token = new CancellationTokenSource();
            CheckAppUpdate(_token.Token).FireAndForgetSafeAsync();
            CheckAddonUpdate(_token.Token).FireAndForgetSafeAsync();
            
        });
    }

    public AsyncCommand SettingsCommand
    {
        get => new AsyncCommand(async (r) =>
        {
            
        });
    }
    
    private async Task UpdateSelectedAsync(CancellationToken token = default)
    {
        _appSettings.CurrentProfile = CurrentProfile.Name;
        await _appSettings.SaveProfileAsync(CurrentProfile);
                
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            name!);
                
        await using var stream =
            new FileStream(Path.Combine(dir, "appsettings.json"), FileMode.Create, FileAccess.Write, FileShare.Write);
        await JsonSerializer.SerializeAsync(stream, _appSettings, cancellationToken: token);
                
        Title = $"Профиль - {_appSettings.CurrentProfile} - {AppVersion}";
    }
    
    private async Task DownloadAndReplace(string downloadUrl, string destinationUrl, CancellationToken token = default)
    {
        using var downloadClient = new HttpClient();

        IsInstalling = true;
        IsDownloading = true;
        
        var responseMessage = await downloadClient.GetAsync(downloadUrl, token);
        var fileName = responseMessage.Content.Headers.ContentDisposition?.FileName;

        if (fileName != null)
        {
            IsInstalling = false;
            await using var stream = new MemoryStream();

            IProgress<float> progress = new Progress<float>(p => { Progress = p; });
            await downloadClient.DownloadAsync(downloadUrl, stream, progress, cancellationToken: token);

            IsInstalling = true;

            await Task.Run(async () =>
            {
                using var arch = new ZipArchive(stream, ZipArchiveMode.Read);

                var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Downloads"));

                if (dir.Exists)
                {
                    foreach (var directoryInfo in dir.GetDirectories())
                    {
                        directoryInfo.Delete(true);
                    }
                }

                if (!dir.Exists)
                {
                    dir.Create();
                }

                arch.ExtractToDirectory(dir.FullName);

                foreach (var dirInfoLocal in dir.GetDirectories())
                {
                    var destDir = new DirectoryInfo(Path.Combine(destinationUrl, dirInfoLocal.Name));

                    if (destDir.Exists)
                    {
                        destDir.Delete(true);
                    }

                    await DirectoryInfoEx.CopyFilesRecursivelyAsync(dirInfoLocal.FullName, destDir.FullName);
                }

                dir.Delete(true);
            }, token);

            IsInstalling = false;
            IsDownloading = false;
        }
    }
    
    private async Task CheckAddonAsync(CancellationToken token = default)
    {
        try
        {
            if(token.IsCancellationRequested) return;

            if (CurrentProfile == null) return;
            
            var pathApp = CurrentProfile.InstallLocation ?? string.Empty;
            var pathType = CurrentProfile.Type == "Retail" ? "_retail_" : "_classic_";

            const string pathAddons = "Interface\\AddOns";

            PathToAddons = Path.Combine(pathApp, pathType, pathAddons);

            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.tukui.org/v1/");
            
            ElvUi = await client.GetFromJsonAsync<ElvUiModel>("addon/elvui", cancellationToken: token);

            if (ElvUi != null)
            {
                var addonName = ElvUi.directories.FirstOrDefault();

                if (!string.IsNullOrEmpty(addonName))
                {
                    var dir = new DirectoryInfo(Path.Combine(PathToAddons, addonName));

                    if (dir.Exists)
                    {
                        IsNeedInstall = false;
                        CurrentProfile.IsInstalled = true;
                        var path = Path.Combine(dir.FullName, "ElvUI_Classic.toc");

                        var fileInfo = new FileInfo(path);

                        if (fileInfo.Exists)
                        {
                            var existedLine =
                                (await File.ReadAllLinesAsync(fileInfo.FullName, token)).FirstOrDefault(c =>
                                    c.Contains("Version"));

                            if (!string.IsNullOrEmpty(existedLine))
                            {
                                var versionStr = existedLine.Replace(" ", string.Empty).Replace("#", string.Empty)
                                    .Split(":")
                                    .LastOrDefault();

                                if (!string.IsNullOrEmpty(versionStr))
                                {
                                    var version = Version.Parse(versionStr);
                                    var versionNew = Version.Parse(ElvUi.version);

                                    if (version >= versionNew)
                                    {
                                        TextVersionInfo = $"Текущая версия {versionNew}";
                                        UpdateText = CurrentProfile.IsForceUpdate ? "Актуально(Обновить)" : "Актуально";
                                        IsNeedUpdate = false;
                                    }
                                    else
                                    {
                                        TextVersionInfo = $"Доступна новая версия {versionNew}";
                                        UpdateText = "Обновить";
                                        IsNeedUpdate = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        TextVersionInfo = "Аддон отсутствует";
                        
                        IsNeedUpdate = false;
                        IsNeedInstall = true;

                        Debug.WriteLine("Аддон отсутствует, нужна установка");
                    }
                }
            }
            else
            {
                IsNeedUpdate = false;
                throw new Exception("информация об аддоне не найден на сайте");
            }
            
            UpdateToolTipText = $"Обновлено {CurrentProfile.Date:dd.MM:yyyy}";
        }
        catch (Exception exception)
        {
            IsNeedUpdate = false;
            Debug.WriteLine(exception.ToString());
        }
    }

    private async Task CheckAppUpdate(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            Debug.WriteLine("Проверка обновления приложения");
            await _updateService.RunAsync(token);
            Debug.WriteLine("Проверка обновления приложения завершена");
            await Delay(AppSettings.UpdateAppInfo, token);
        }
        
        Debug.WriteLine("Проверка обновления приложения отмена");
    }
    
    private async Task CheckAddonUpdate(CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            Debug.WriteLine("Проверка обновления аддона");

            IsBusy = true;
            
            await CheckAddonAsync(token);

            IsBusy = false;

            //if (IsNeedInstall || IsNeedUpdate)
            //{
                var processes = Process.GetProcesses().Where(p => string.Equals(p.ProcessName, "Wow", StringComparison.CurrentCultureIgnoreCase)).ToList();

                if (processes.Count > 0)
                {
                    foreach (var process in processes)
                    {
                        if (CurrentProfile != null)
                        {
                            if (process.MainModule != null)
                            {
                                if (!string.IsNullOrEmpty(process.MainModule.FileName) &&
                                    !string.IsNullOrEmpty(CurrentProfile.InstallLocation))
                                {
                                    if (process.MainModule.FileName.Contains(CurrentProfile.InstallLocation))
                                    {
                                        var processId = process.Id;
                                        Debug.WriteLine($"{process.Id} - {process.MainModule.FileName} - запущена");
                                        await process.WaitForExitAsync(token);
                                        Debug.WriteLine($"{processId} - exited");
                                    }
                                }
                            }
                        }
                    }
                }
                //}
            
            if (IsNeedInstall)
            {
                Debug.WriteLine("Проверка установки аддона");
                await InstallAddonAsync(token);
                Debug.WriteLine("Аддон установлен");
            }

            if (IsNeedUpdate)
            {
                Debug.WriteLine("Требуется обновление аддона");
                await UpdateAddonAsync(token);
                Debug.WriteLine("Аддон обновлен");
            }

            Debug.WriteLine("Проверка обновления аддона завершена");
            await Delay(AppSettings.UpdateAddonInfo, token);
        }
        
        Debug.WriteLine("Проверка обновления аддона отмена");
    }

    private async Task Delay(UpdateInfoModel model, CancellationToken token = default)
    {
        switch (model.Type)
        {
            case "FromMilliseconds":
            {
                await Task.Delay(TimeSpan.FromMilliseconds(model.Interval), token);
                break;               
            }
            case "Minutes":
            {
                await Task.Delay(TimeSpan.FromMinutes(model.Interval), token);
                break;
            }
            case "Seconds":
            {
                await Task.Delay(TimeSpan.FromSeconds(model.Interval), token);
                break;
            }
            case "Hours":
            {
                await Task.Delay(TimeSpan.FromHours(model.Interval), token);
                break;               
            }
            case "Days":
            {
                await Task.Delay(TimeSpan.FromDays(model.Interval), token);
                break;               
            }
            default:
            {
                await Task.Delay(TimeSpan.FromMinutes(model.Interval), token);
                break;  
            }
        }
    }

    public async Task UpdateAddonAsync(CancellationToken token = default)
    {
        if (ElvUi != null)
        {
            if(token.IsCancellationRequested) return;
            
            await DownloadAndReplace(ElvUi.url, PathToAddons, token);

            CurrentProfile.Date = DateTime.Now;
            CurrentProfile.Version = ElvUi.version;

            if(token.IsCancellationRequested) return;
            
            await AppSettings.SaveProfileAsync(CurrentProfile);

            await CheckAddonAsync(token);
        }
    }

    public async Task InstallAddonAsync(CancellationToken token = default)
    {
        if (ElvUi != null)
        {
            if(token.IsCancellationRequested) return;
            
            await DownloadAndReplace(ElvUi.url, PathToAddons, token);

            if (CurrentProfile != null)
            {
                CurrentProfile.Date = DateTime.Now;
                CurrentProfile.Version = ElvUi.version;

                if (token.IsCancellationRequested) return;

                await AppSettings.SaveProfileAsync(CurrentProfile);
            }

            await CheckAddonAsync(token);
        }
    }
}