using ElvUIDownloader.Hosted.Base;
using ElvUIDownloader.Models;
using ElvUIDownloader.Services;
using ElvUIDownloader.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ElvUIDownloader.Hosted;

/// <summary>
/// 
/// </summary>
public class UpdateAddonBackgroundService : BaseBackgroundService
{
    private readonly AddonStore _updateAddonStore;
    private readonly AppSettings _appSettings;
    private readonly UpdateAddonService _updateAddonService;

    public UpdateAddonBackgroundService(
        ILogger<UpdateAddonBackgroundService> logger,
        AddonStore updateAddonStore,
        AppSettings appsettings,
        UpdateAddonService updateAddonService) : base(logger)
    {
        WaitingTimeAfterCompleted = TimeSpan.FromSeconds(1);
        _updateAddonStore = updateAddonStore;
        _updateAddonService = updateAddonService;

        _appSettings = appsettings;
        _appSettings.UpdateAddonInfo.PropertyChanged += UpdateAddonInfo_PropertyChanged;
        _updateAddonStore.ElapsedTime.Time = _appSettings.UpdateAddonInfo.GetTimeInterval();
    }

    private void UpdateAddonInfo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Logger.LogInformation($"{sender.GetType()} {e.PropertyName} - changed");

        //if(e.PropertyName == nameof(AppSettings.UpdateAddonInfo.Interval))
        //{
            _updateAddonStore.ElapsedTime.Time = _appSettings.UpdateAddonInfo.GetTimeInterval();
        //}
    }

    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_updateAddonStore.ElapsedTime.Time.TotalSeconds > 0)
        {
            return;
        }

        Logger.LogInformation("Выполнение проверки аддона");

        await _updateAddonService.CheckAsync(cancellationToken);

        Logger.LogInformation("Проверка аддона завершена");

        _updateAddonStore.ElapsedTime.Time = _appSettings.UpdateAddonInfo.GetTimeInterval();
        _updateAddonStore.ElapsedTime.Start();
    }
}