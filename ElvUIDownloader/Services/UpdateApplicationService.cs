using ElvUIDownloader.Models;
using ElvUIDownloader.Stores;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using YandexDisk.API.Client.Contracts;

namespace ElvUIDownloader.Services;

public class UpdateApplicationService
{
    private readonly ILogger<UpdateApplicationService> _logger;
    private readonly IYandexDiskApi _diskApi;
    private readonly ApplicationStore _applicationStore;

    public UpdateApplicationService(
        ILogger<UpdateApplicationService> logger,
        ApplicationStore applicationSrtore,
        IYandexDiskApi diskApi)
    {
        _logger = logger;
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

                // Setup всегда скачиваем
                if (!_applicationStore.LocalSetupFilename.Exists)
                {
                    using var setupStream = await _diskApi.DownloadFileAsync(_applicationStore.RemoteSetupFilename, cancellationToken);
                    if (setupStream == null) return;

                    await using var fs = new FileStream(_applicationStore.LocalSetupFilename.FullName, FileMode.Create, FileAccess.Write);
                    await setupStream.CopyToAsync(fs, cancellationToken);
                }

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
                                _logger.LogInformation("Новая версия установщика уже скачана. Загружать новую не требуется");
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
                        _logger.LogInformation($"Файл установки не найден {_applicationStore.LocalSetupFilename}");
                        _applicationStore.IsNeedUpdate = false;
                        return;
                    }

                    _applicationStore.IsNeedUpdate = true;

                    _logger.LogInformation("Обновление необходимо");
                }
                else
                {
                    if(updateVersion == _applicationStore.Version)
                    {
                        _logger.LogInformation($"Версия обновления ({updateVersion}) актуальная - Текущая версия ({_applicationStore.Version}) ");
                    }
                    else
                    {
                        _logger.LogInformation($"Версия обновления ({updateVersion}) ниже - Текущая версия ({_applicationStore.Version}) ");
                    }

                    _applicationStore.IsNeedUpdate = false;
                }
            }
            else
            {
                _applicationStore.IsNeedUpdate = false;
                _logger.LogError("Не удалось получить информацию о версии");
            }
        }
        catch (Exception e)
        {
            _applicationStore.IsNeedUpdate = false;
            _logger.LogError("{ex}", e);
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
        if (_applicationStore.LocalSetupFilename.Exists)
        {
            await Task.Run(() =>
            {
                _logger.LogInformation($"Подготовка к установке {_applicationStore.LocalSetupFilename.Exists}");

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

                _logger.LogInformation($"Запуск установки {_applicationStore.LocalSetupFilename.Exists}");

                if (process.Start())
                {
                    _logger.LogInformation($"Установка запущена");
                }
                else
                {
                    _logger.LogError($"Ошибка запуска установки");
                }

            }, cancellationToken);
        }
    }
}