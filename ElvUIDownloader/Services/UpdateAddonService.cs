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

public class UpdateAddonService(
    AddonStore addonStore,
    AppSettings appSettings,
    ProfileStore profileStore,
    IHttpClientFactory clientFactory)
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        // Wait asynchronously to enter the critical section.
        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (profileStore.CurrentProfile == null) return;

            var pathApp = profileStore.CurrentProfile.InstallLocation ?? string.Empty;
            var pathType = profileStore.CurrentProfile.Type == EGameType.Retail ? "_retail_" : "_classic_";

            const string pathAddons = "Interface\\AddOns";

            addonStore.PathToAddons = Path.Combine(pathApp, pathType, pathAddons);

            using var client = clientFactory.CreateClient("TukuiClient");

            addonStore.ElvUi = await client.GetFromJsonAsync<ElvUiJsonModel>("addon/elvui", cancellationToken: cancellationToken);

            if (addonStore.ElvUi != null)
            {
                var addonName = addonStore.ElvUi.Directories.FirstOrDefault();

                if (!string.IsNullOrEmpty(addonName))
                {
                    var dir = new DirectoryInfo(Path.Combine(addonStore.PathToAddons, addonName));

                    if (dir.Exists)
                    {
                        addonStore.Mode = EAddonMode.None;

                        const string name = "ElvUI";
                        const string ext = "toc";

                        var path = Path.Combine(dir.FullName, $"{name}_Classic.{ext}");
                        var pathMainLine = Path.Combine(dir.FullName, $"{name}.{ext}");
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

                        profileStore.CurrentProfile.IsInstalled = true;
                        addonStore.InstallTime = dir.CreationTime;

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
                                    var versionNew = Version.Parse(addonStore.ElvUi.Version);
                                    
                                    profileStore.CurrentProfile.Date = DateTime.Parse(addonStore.ElvUi.LastUpdate);
                                    profileStore.CurrentProfile.Version = version.ToString();
                                    
                                    if (version >= versionNew)
                                    {
                                        addonStore.TextVersionInfo = $"Текущая версия {versionNew}";
                                        addonStore.UpdateText = profileStore.CurrentProfile.IsForceUpdate ? "Актуально(Обновить)" : "Актуально";
                                        addonStore.Mode = EAddonMode.Installed;
                                    }
                                    else
                                    {
                                        addonStore.TextVersionInfo = $"Доступна новая версия {versionNew}";
                                        addonStore.UpdateText = "Обновить";
                                        addonStore.Mode = EAddonMode.NeedUpdate;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        addonStore.TextVersionInfo = "Аддон отсутствует";
                        
                        addonStore.Mode = EAddonMode.NeedInstall;

                        Debug.WriteLine("Аддон отсутствует, нужна установка");
                    }
                }
            }
            else
            {
                addonStore.Mode = EAddonMode.None;
                throw new Exception("информация об аддоне не найдена на сайте");
            }

            addonStore.UpdateToolTipText = $"Обновлено {profileStore.CurrentProfile.Date:dd.MM.yyyy}";
        }
        catch (Exception exception)
        {
            addonStore.Mode = EAddonMode.None;
            Debug.WriteLine(exception.ToString());
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task InstallAsync(CancellationToken cancellationToken = default)
    {
        addonStore.IsUpdateAddonTaskRunning = true;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (addonStore.Mode)
            {
                case EAddonMode.NeedInstall:
                    Debug.WriteLine("Проверка установки аддона");

                    await InternalInstallAsync(cancellationToken);
                    Debug.WriteLine("Аддон установлен");
                    break;
                case EAddonMode.NeedUpdate:
                    Debug.WriteLine("Требуется обновление аддона");

                    await InternalInstallAsync(cancellationToken);
                    Debug.WriteLine("Аддон обновлен");
                    break;
            }
        }
        finally
        {
            addonStore.IsUpdateAddonTaskRunning = false;
            addonStore.IsInstalling = false;
            addonStore.IsDownloading = false;
        }

        Debug.WriteLine("Проверка обновления аддона завершена");
    }

    private async Task InternalInstallAsync(CancellationToken cancellationToken = default)
    {
        if (addonStore.ElvUi != null)
        {
            if (cancellationToken.IsCancellationRequested) return;

            await DownloadAndReplace(addonStore.ElvUi.Url, addonStore.PathToAddons, cancellationToken);

            if (profileStore.CurrentProfile != null)
            {
                profileStore.CurrentProfile.Date = DateTime.Now;
                profileStore.CurrentProfile.Version = addonStore.ElvUi.Version;

                if (cancellationToken.IsCancellationRequested) return;

                await appSettings.SaveProfileAsync(profileStore.CurrentProfile, cancellationToken);
            }
        }
    }

    private async Task DownloadAndReplace(string downloadUrl, string destinationUrl, CancellationToken cancellationToken = default)
    {
        using var downloadClient = clientFactory.CreateClient("TukuiClient");

        addonStore.IsInstalling = true;
        addonStore.IsDownloading = true;

        var responseMessage = await downloadClient.GetAsync(downloadUrl, cancellationToken);
        var fileName = responseMessage.Content.Headers.ContentDisposition?.FileName;

        if (fileName != null)
        {
            addonStore.IsInstalling = false;
            await using var stream = new MemoryStream();

            IProgress<float> progress = new Progress<float>(p => { addonStore.Progress = p; });
            await downloadClient.DownloadAsync(downloadUrl, stream, progress, cancellationToken: cancellationToken);

            addonStore.IsInstalling = true;

            await Task.Run(async () =>
            {
                await using var arch = new ZipArchive(stream, ZipArchiveMode.Read);

                var extractionPath = Path.Combine(
                    Path.GetTempPath(),
                    "ElvUIDownloader",
                    Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(extractionPath);

                try
                {
                    await arch.ExtractToDirectoryAsync(extractionPath, cancellationToken);

                    foreach (var dirInfoLocal in Directory.GetDirectories(extractionPath))
                    {
                        var addonName = Path.GetFileName(dirInfoLocal);
                        var destDir = new DirectoryInfo(Path.Combine(destinationUrl, addonName));

                        if (destDir.Exists)
                        {
                            destDir.Delete(true);
                        }

                        await DirectoryInfoEx.CopyFilesRecursivelyAsync(dirInfoLocal, destDir.FullName);
                    }
                }
                finally
                {
                    if (Directory.Exists(extractionPath))
                    {
                        Directory.Delete(extractionPath, true);
                    }
                }
            }, cancellationToken);

            addonStore.IsInstalling = false;
            addonStore.IsDownloading = false;
        }
    }
}
