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
public class InstallAddonBackgroundService : BaseBackgroundService
{
    private readonly AddonStore _addonStore;
    private readonly UpdateAddonService _updateAddonService;

    public InstallAddonBackgroundService(
        ILogger<InstallAddonBackgroundService> logger,
        AddonStore addonStore,
        UpdateAddonService updateAddonService) : base(logger)
    {
        //WaitingTimeAfterCompleted = TimeSpan.FromSeconds(1);
        _addonStore = addonStore;
        _updateAddonService = updateAddonService;

        UseLogging = true;
        WaitingTimeAfterCompleted = TimeSpan.FromMinutes(5);
    }

    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_addonStore.Mode == EAddonMode.NeedInstall || _addonStore.Mode == EAddonMode.NeedUpdate)
        {
            Logger.LogInformation("Выполнение запуска установки аддона");

            await _updateAddonService.InstallAsync(cancellationToken);

            Logger.LogInformation("Установка аддона завершена");
        }
        else
        {
            Logger.LogInformation("Обновление аддона не нужно. Актуальная версия");
        }
    }
}