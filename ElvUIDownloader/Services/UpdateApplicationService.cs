using ElvUIDownloader.Stores;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using ElvUIDownloader.Models.Web;
using YandexDisk.API.Client.Contracts;

namespace ElvUIDownloader.Services;

public class UpdateApplicationService(
    ILogger<UpdateApplicationService> logger,
    ApplicationStore applicationStore,
    IYandexDiskApi diskApi)
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        applicationStore.LocalSetupFilename.Refresh();

        // Wait asynchronously to enter the critical section.
        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (cancellationToken.IsCancellationRequested) return;

            var stream =
                await diskApi.DownloadFileAsync(applicationStore.RemoteFileVersionFilename, cancellationToken);

            if (stream == null)
            {
                applicationStore.IsNeedUpdate = false;
                return;
            }

            var data = await JsonSerializer.DeserializeAsync<AppVersionJsonModel>(stream, cancellationToken: cancellationToken);

            if (data != null)
            {
                var updateVersion = Version.Parse(data.FileVersion);
                applicationStore.UpdateVersion = updateVersion;

                // Setup всегда скачиваем
                if (!applicationStore.LocalSetupFilename.Exists)
                {
                    await using var setupStream = await diskApi.DownloadFileAsync(applicationStore.RemoteSetupFilename, cancellationToken);
                    if (setupStream == null) return;

                    await using var fs = new FileStream(applicationStore.LocalSetupFilename.FullName, FileMode.Create, FileAccess.Write);
                    await setupStream.CopyToAsync(fs, cancellationToken);
                }

                if (applicationStore.UpdateVersion > applicationStore.Version)
                {
                    applicationStore.LocalSetupFilename.Refresh();

                    if (applicationStore.LocalSetupFilename.Exists)
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(applicationStore.LocalSetupFilename.FullName);

                        if (versionInfo.FileVersion != null)
                        {
                            var localSetupVersion = Version.Parse(versionInfo.FileVersion);

                            // Если версия установщика меньше или равна, то его нужно удалить
                            // Или версия на сервере больше скачанной версии
                            if (localSetupVersion <= applicationStore.Version || applicationStore.UpdateVersion > localSetupVersion)
                            {
                                applicationStore.LocalSetupFilename.Delete();
                            }
                            else
                            {
                                logger.LogInformation("Новая версия установщика уже скачана. Загружать новую не требуется");
                            }
                        }
                    }

                    applicationStore.LocalSetupFilename.Refresh();

                    if (!applicationStore.LocalSetupFilename.Exists)
                    {
                        await using var setupStream = await diskApi.DownloadFileAsync(applicationStore.RemoteSetupFilename, cancellationToken);
                        if (setupStream == null) return;

                        await using var fs = new FileStream(applicationStore.LocalSetupFilename.FullName, FileMode.Create, FileAccess.Write);
                        await setupStream.CopyToAsync(fs, cancellationToken);
                    }

                    applicationStore.LocalSetupFilename.Refresh();

                    if (!applicationStore.LocalSetupFilename.Exists)
                    {
                        logger.LogInformation("Файл установки не найден {ApplicationStoreLocalSetupFilename}", applicationStore.LocalSetupFilename);
                        applicationStore.IsNeedUpdate = false;
                        return;
                    }

                    applicationStore.IsNeedUpdate = true;

                    logger.LogInformation("Обновление необходимо");
                }
                else
                {
                    if(applicationStore.UpdateVersion == applicationStore.Version)
                    {
                        logger.LogInformation("Версия обновления ({UpdateVersion}) актуальная - Текущая версия ({ApplicationStoreVersion}) ", applicationStore.UpdateVersion, applicationStore.Version);
                    }
                    else
                    {
                        logger.LogInformation("Версия обновления ({UpdateVersion}) ниже - Текущая версия ({ApplicationStoreVersion}) ", applicationStore.UpdateVersion, applicationStore.Version);
                    }

                    applicationStore.IsNeedUpdate = false;
                }
            }
            else
            {
                applicationStore.IsNeedUpdate = false;
                logger.LogError("Не удалось получить информацию о версии");
            }
        }
        catch (Exception e)
        {
            applicationStore.IsNeedUpdate = false;
            logger.LogError(e, "");
        }
        finally
        {
            Semaphore.Release();
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
        applicationStore.LocalSetupFilename.Refresh();
        if (applicationStore.LocalSetupFilename.Exists)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Подготовка к установке {Exists}", applicationStore.LocalSetupFilename.Exists);

                const string silent = "/VERYSILENT";

                var processInfo = new ProcessStartInfo
                {
                    FileName = applicationStore.LocalSetupFilename.FullName,
                };

                if (isSilentInstall)
                {
                    processInfo.Arguments = $"{silent}";
                }

                var process = new Process
                {
                    StartInfo = processInfo
                };

                logger.LogInformation("Запуск установки {Exists}", applicationStore.LocalSetupFilename.Exists);

                if (process.Start())
                {
                    logger.LogInformation($"Установка запущена");
                }
                else
                {
                    logger.LogError($"Ошибка запуска установки");
                }

            }, cancellationToken);
        }
    }
}