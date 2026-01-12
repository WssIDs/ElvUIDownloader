using ElvUIDownloader.Hosted.Base;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ElvUIDownloader.Hosted;

/// <summary>
/// 
/// </summary>
public class InstallApplicationBackgroundService : BaseBackgroundService
{
    private readonly ApplicationStore _applicationStore;
    private readonly UpdateApplicationService _updateApplicationService;

    public InstallApplicationBackgroundService(
        ILogger<InstallApplicationBackgroundService> logger,
        ApplicationStore applicationStore,
        UpdateApplicationService updateApplicationService) : base(logger)
    {
        WaitingTimeAfterCompleted = TimeSpan.FromSeconds(1);
        _applicationStore = applicationStore;
        _updateApplicationService = updateApplicationService;

        WaitingTimeAfterCompleted = TimeSpan.FromMinutes(5);
    }

    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_applicationStore.IsNeedUpdate)
        {
            Logger.LogInformation("Выполнение запуска установки приложения");

            await _updateApplicationService.InstallAsync(true, cancellationToken);

            Logger.LogInformation("Установка приложения завершена");
        }
        else
        {
            Logger.LogInformation("Обновление приложение не нужно. Актуальная версия");
        }
    }
}