using ElvUIDownloader.Models;
using ElvUIDownloader.Stores;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using YandexDisk.API.Client.Contracts;

namespace ElvUIDownloader.Services;

public class UpdateApplicationService
{
    private readonly IYandexDiskApi _diskApi;
    private readonly ApplicationStore _applicationStore;

    public UpdateApplicationService(
        ApplicationStore applicationSrtore,
        IYandexDiskApi diskApi)
    {
        _diskApi = diskApi;
        _applicationStore = applicationSrtore;
    }

    private static readonly SemaphoreSlim semaphore = new(1, 1);

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        // Wait asynchronously to enter the critical section.
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (cancellationToken.IsCancellationRequested) return;

            var stream =
                await _diskApi.DownloadFileAsync(_applicationStore.RemoteFileVersionFilename, cancellationToken);

            if (stream == null)
            {
                _applicationStore.IsNeedUpdate = false;
                return;
            }

            var data = await JsonSerializer.DeserializeAsync<AppVersionModel>(stream, cancellationToken: cancellationToken);

            if (data != null)
            {
                var updateVersion = Version.Parse(data.FileVersion);
                
                if (updateVersion > _applicationStore.Version)
                {
                    if (_applicationStore.LocalSetupFilename.Exists)
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(_applicationStore.LocalSetupFilename.FullName);

                        if (versionInfo.FileVersion != null)
                        {
                            var localSetupVersion = Version.Parse(versionInfo.FileVersion);

                            // Если версия установщика меньше или равна, то его нужно удалить
                            // Или версия на сервере больше скачанной версии
                            if (localSetupVersion <= _applicationStore.Version || updateVersion > localSetupVersion)
                            {
                                _applicationStore.LocalSetupFilename.Delete();
                            }
                            else
                            {
                                Debug.WriteLine("Новая версия установщика уже скачана. Загружать новую не требуется");
                            }
                        }
                    }

                    if (!_applicationStore.LocalSetupFilename.Exists)
                    {
                        using var setupStream = await _diskApi.DownloadFileAsync(_applicationStore.RemoteSetupFilename, cancellationToken);
                        if (setupStream == null) return;

                        await using var fs = new FileStream(_applicationStore.LocalSetupFilename.FullName, FileMode.Create, FileAccess.Write);
                        await setupStream.CopyToAsync(fs, cancellationToken);
                    }

                    if (!_applicationStore.LocalSetupFilename.Exists)
                    {
                        Debug.WriteLine($"Файл установки не найден {_applicationStore.LocalSetupFilename}");
                        _applicationStore.IsNeedUpdate = false;
                        return;
                    }

                    _applicationStore.IsNeedUpdate = true;

                    Debug.WriteLine("Обновление необходимо");
                }
                else
                {
                    Debug.WriteLine($"Версия обновления ({updateVersion}) актуальная или ниже - Текущая версия ({_applicationStore.Version}) ");
                    _applicationStore.IsNeedUpdate = false;
                }
            }
            else
            {
                _applicationStore.IsNeedUpdate = false;
                Debug.WriteLine("Не удалось получить информацию о версии");
            }
        }
        catch (Exception e)
        {
            _applicationStore.IsNeedUpdate = false;
            Debug.WriteLine(e);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Установить обновление (запуск установщика)
    /// </summary>
    /// <param name="isSilentInstall">тихая установка</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InstallAsync(bool isSilentInstall = true, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            const string silent = "/VERYSILENT";

            var processInfo = new ProcessStartInfo
            {
                FileName = _applicationStore.LocalSetupFilename.FullName,
            };

            if (isSilentInstall)
            {
                processInfo.Arguments = $"{silent}";
            }

            var process = new Process
            {
                StartInfo = processInfo
            };

            process.Start();
        }, cancellationToken);
    }
}