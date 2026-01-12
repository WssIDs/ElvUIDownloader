using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Web;
using ElvUIDownloader.Stores;
using ElvUIDownloader.Utils;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;

namespace ElvUIDownloader.Services;

public class UpdateAddonService
{
    private readonly AppSettings _appSettings;
    private readonly AddonStore _addonStore;
    private readonly ProfileStore _profileStore;
    private readonly IHttpClientFactory _clientFactory;

    public UpdateAddonService(
        AddonStore addonStore,
        AppSettings appSettings,
        ProfileStore profileStore,
        IHttpClientFactory clientFactory)
    {
        _addonStore = addonStore;
        _profileStore = profileStore;
        _clientFactory = clientFactory;
        _appSettings = appSettings;
    }

    private static readonly SemaphoreSlim semaphore = new(1, 1);

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        // Wait asynchronously to enter the critical section.
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_profileStore.CurrentProfile == null) return;

            var pathApp = _profileStore.CurrentProfile.InstallLocation ?? string.Empty;
            var pathType = _profileStore.CurrentProfile.Type == EGameType.Retail ? "_retail_" : "_classic_";

            const string pathAddons = "Interface\\AddOns";

            _addonStore.PathToAddons = Path.Combine(pathApp, pathType, pathAddons);

            using var client = _clientFactory.CreateClient("TukuiClient");

            _addonStore.ElvUi = await client.GetFromJsonAsync<ElvUiModel>("addon/elvui", cancellationToken: cancellationToken);

            if (_addonStore.ElvUi != null)
            {
                var addonName = _addonStore.ElvUi.Directories.FirstOrDefault();

                if (!string.IsNullOrEmpty(addonName))
                {
                    var dir = new DirectoryInfo(Path.Combine(_addonStore.PathToAddons, addonName));

                    if (dir.Exists)
                    {
                        _addonStore.Mode = EAddonMode.None;

                        var name = "ElvUI";
                        var ext = "toc";

                        var path = Path.Combine(dir.FullName, $"{name}_Classic.{ext}");
                        var pathMainLine = Path.Combine(dir.FullName, $"{name}_Mainline.{ext}");
                        var pathCata = Path.Combine(dir.FullName, $"{name}_Cata.{ext}");
                        var pathVanila = Path.Combine(dir.FullName, $"{name}_Vanilla.{ext}");

                        var paths = new List<FileInfo>
                        {
                            new(path),
                            new(pathCata),
                            new(pathMainLine),
                            new(pathVanila),
                        };

                        var fileInfo = paths.FirstOrDefault(f => f.Exists);

                        _profileStore.CurrentProfile.IsInstalled = true;
                        _addonStore.InstallTime = dir.CreationTime;

                        if (fileInfo != null)
                        {
                            var existedLine =
                                (await File.ReadAllLinesAsync(fileInfo.FullName, cancellationToken))
                                .FirstOrDefault(c =>
                                    c.Contains("Version"));

                            if (!string.IsNullOrEmpty(existedLine))
                            {
                                var versionStr = existedLine
                                    .Replace(" ", string.Empty)
                                    .Replace("#", string.Empty)
                                    .Replace("v", string.Empty)
                                    .Split(":")
                                    .LastOrDefault();

                                if (!string.IsNullOrEmpty(versionStr))
                                {
                                    var version = Version.Parse(versionStr);
                                    var versionNew = Version.Parse(_addonStore.ElvUi.Version);

                                    //if(ProfileStore.CurrentProfile.Date == null)
                                    //{
                                    _profileStore.CurrentProfile.Date = DateTime.Parse(_addonStore.ElvUi.LastUpdate);
                                    _profileStore.CurrentProfile.Version = version.ToString();
                                    //}

                                    if (version >= versionNew)
                                    {
                                        _addonStore.TextVersionInfo = $"Текущая версия {versionNew}";
                                        _addonStore.UpdateText = _profileStore.CurrentProfile.IsForceUpdate ? "Актуально(Обновить)" : "Актуально";
                                        _addonStore.Mode = EAddonMode.Installed;
                                    }
                                    else
                                    {
                                        _addonStore.TextVersionInfo = $"Доступна новая версия {versionNew}";
                                        _addonStore.UpdateText = "Обновить";
                                        _addonStore.Mode = EAddonMode.NeedUpdate;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _addonStore.TextVersionInfo = "Аддон отсутствует";

                        //IsNeedUpdate = false;
                        //IsNeedInstall = true;
                        _addonStore.Mode = EAddonMode.NeedInstall;

                        Debug.WriteLine("Аддон отсутствует, нужна установка");
                    }
                }
            }
            else
            {
                //IsNeedUpdate = false;
                _addonStore.Mode = EAddonMode.None;
                throw new Exception("информация об аддоне не найдена на сайте");
            }

            _addonStore.UpdateToolTipText = $"Обновлено {_profileStore.CurrentProfile.Date:dd.MM:yyyy}";
        }
        catch (Exception exception)
        {
            ///IsNeedUpdate = false;
            _addonStore.Mode = EAddonMode.None;
            Debug.WriteLine(exception.ToString());
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task InstallAsync(CancellationToken cancellationToken = default)
    {
        _addonStore.IsUpdateAddonTaskRunning = true;

        cancellationToken.ThrowIfCancellationRequested();

        if (_addonStore.Mode == EAddonMode.NeedInstall)
        {
            Debug.WriteLine("Проверка установки аддона");

            var processes = Process.GetProcesses().Where(p =>
                string.Equals(p.ProcessName, "Wow", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (processes.Count > 0)
            {
                foreach (var process in processes)
                {
                    if (_profileStore.CurrentProfile == null ||
                        process.MainModule == null ||
                        string.IsNullOrEmpty(process.MainModule.FileName) ||
                        string.IsNullOrEmpty(_profileStore.CurrentProfile.InstallLocation)) continue;
                    if (!process.MainModule.FileName.Contains(_profileStore.CurrentProfile.InstallLocation)) continue;
                    var processId = process.Id;
                    Debug.WriteLine($"{process.Id} - {process.MainModule.FileName} - запущена");
                    await process.WaitForExitAsync(cancellationToken);
                    Debug.WriteLine($"{processId} - exited");
                }
            }

            await InternalInstallAsync(cancellationToken);
            Debug.WriteLine("Аддон установлен");
        }

        if (_addonStore.Mode == EAddonMode.NeedUpdate)
        {
            Debug.WriteLine("Требуется обновление аддона");

            var processes = Process.GetProcesses().Where(p =>
                string.Equals(p.ProcessName, "Wow", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (processes.Count > 0)
            {
                foreach (var process in processes)
                {
                    if (_profileStore.CurrentProfile == null ||
                        process.MainModule == null ||
                        string.IsNullOrEmpty(process.MainModule.FileName) ||
                        string.IsNullOrEmpty(_profileStore.CurrentProfile.InstallLocation) ||
                        !process.MainModule.FileName.Contains(_profileStore.CurrentProfile.InstallLocation)) continue;
                    var processId = process.Id;
                    Debug.WriteLine($"{process.Id} - {process.MainModule.FileName} - запущена");
                    await process.WaitForExitAsync(cancellationToken);
                    Debug.WriteLine($"{processId} - exited");
                }
            }

            await InternalInstallAsync(cancellationToken);
            Debug.WriteLine("Аддон обновлен");
        }

        Debug.WriteLine("Проверка обновления аддона завершена");

        _addonStore.IsUpdateAddonTaskRunning = false;
    }

    private async Task InternalInstallAsync(CancellationToken cancellationToken = default)
    {
        if (_addonStore.ElvUi != null)
        {
            if (cancellationToken.IsCancellationRequested) return;

            await DownloadAndReplace(_addonStore.ElvUi.Url, _addonStore.PathToAddons, cancellationToken);

            if (_profileStore.CurrentProfile != null)
            {
                _profileStore.CurrentProfile.Date = DateTime.Now;
                _profileStore.CurrentProfile.Version = _addonStore.ElvUi.Version;

                if (cancellationToken.IsCancellationRequested) return;

                await _appSettings.SaveProfileAsync(_profileStore.CurrentProfile, cancellationToken);
            }
        }
    }

    private async Task DownloadAndReplace(string downloadUrl, string destinationUrl, CancellationToken cancellationToken = default)
    {
        using var downloadClient = _clientFactory.CreateClient("TukuiClient");

        _addonStore.IsInstalling = true;
        _addonStore.IsDownloading = true;

        var responseMessage = await downloadClient.GetAsync(downloadUrl, cancellationToken);
        var fileName = responseMessage.Content.Headers.ContentDisposition?.FileName;

        if (fileName != null)
        {
            _addonStore.IsInstalling = false;
            await using var stream = new MemoryStream();

            IProgress<float> progress = new Progress<float>(p => { _addonStore.Progress = p; });
            await downloadClient.DownloadAsync(downloadUrl, stream, progress, cancellationToken: cancellationToken);

            _addonStore.IsInstalling = true;

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
            }, cancellationToken);

            _addonStore.IsInstalling = false;
            _addonStore.IsDownloading = false;
        }
    }
}