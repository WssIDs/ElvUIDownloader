using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ElvUIDownloader.Hosted.Base;

public abstract class BaseBackgroundService(ILogger<BaseBackgroundService> logger) : BackgroundService
{
    protected ILogger<BaseBackgroundService> Logger { get; } = logger;

    public TimeSpan WaitingTimeAfterCompleted = TimeSpan.FromMilliseconds(250);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                //Logger.LogInformation("Запуск задачи: {Time}", DateTimeOffset.Now);

                await RunAsync(cancellationToken);

                await Task.Delay(WaitingTimeAfterCompleted, cancellationToken);

                //Logger.LogInformation("Задача выполнена успешно: {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка - {Exception}", ex);
                Logger.LogInformation("Задача завершилась с ошибкой: {Time}", DateTimeOffset.Now);
            }
        }
    }

    public abstract Task RunAsync(CancellationToken cancellationToken = default);
}