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
public class UpdateApplicationBackgroundService : BaseBackgroundService
{
    private readonly ApplicationStore _applicationStore;
    private readonly AppSettings _appSettings;
    private readonly UpdateApplicationService _updateApplicationService;

    public UpdateApplicationBackgroundService(
        ILogger<UpdateApplicationBackgroundService> logger,
        ApplicationStore applicationStore,
        AppSettings appsettings,
        UpdateApplicationService updateApplicationService) : base(logger)
    {
        WaitingTimeAfterCompleted = TimeSpan.FromSeconds(1);
        _applicationStore = applicationStore;
        _updateApplicationService = updateApplicationService;

        _appSettings = appsettings;
        _appSettings.UpdateAppInfo.PropertyChanged += UpdateApplicationInfo_PropertyChanged;
        _applicationStore.ElapsedTime.Time = _appSettings.UpdateAppInfo.GetTimeInterval();
    }

    private void UpdateApplicationInfo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Logger.LogInformation($"{sender.GetType()} {e.PropertyName} - changed");

        if(e.PropertyName == nameof(AppSettings.UpdateAppInfo.Interval))
        {
            _applicationStore.ElapsedTime.Time = _appSettings.UpdateAppInfo.GetTimeInterval();
        }
    }

    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_applicationStore.ElapsedTime.Time.TotalSeconds > 0)
        {
            return;
        }

        Logger.LogInformation("Выполнение проверки приложения");

        await _updateApplicationService.CheckAsync(cancellationToken);

        Logger.LogInformation("Проверка приложения завершена");

        _applicationStore.ElapsedTime.Time = _appSettings.UpdateAppInfo.GetTimeInterval();
        _applicationStore.ElapsedTime.Start();
    }
}