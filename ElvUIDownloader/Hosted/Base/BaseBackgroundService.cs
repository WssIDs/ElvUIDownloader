using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ElvUIDownloader.Hosted.Base;

public abstract class BaseBackgroundService : BackgroundService
{
    protected bool StartAfterRun { get; set; } = false;

    private bool _isFirstRun = true;

    public string TaskName { get; set; } = string.Empty;

    public bool UseLogging { get; set; } = false;

    protected ILogger<BaseBackgroundService> Logger { get; }

    public TimeSpan WaitingTimeAfterCompleted = TimeSpan.FromMilliseconds(250);

    public BaseBackgroundService(ILogger<BaseBackgroundService> logger)
    {
        Logger = logger;

        TaskName = $"{GetType().Name}_{Guid.NewGuid()}";
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!StartAfterRun)
                {
                    if (_isFirstRun)
                    {
                        _isFirstRun = false;

                        if (UseLogging)
                        {
                            Logger.LogInformation("[{TaskName}] Пропускаем запуск после старта", TaskName);
                        }
                    }
                    else
                    {
                        if (UseLogging)
                        {
                            Logger.LogInformation("[{TaskName}] Запуск: [{Time}]", TaskName, DateTimeOffset.Now);
                        }

                        await RunAsync(cancellationToken);
                    }
                }
                else
                {
                    if (UseLogging)
                    {
                        Logger.LogInformation("[{TaskName}] Запуск: [{Time}]", TaskName, DateTimeOffset.Now);
                    }

                    await RunAsync(cancellationToken);
                }

                if (UseLogging)
                {
                    Logger.LogInformation("[{TaskName}] Задача выполнена успешно: [{Time}]. Следующий запуск через: [{NextTime}]", TaskName, DateTimeOffset.Now, WaitingTimeAfterCompleted);
                }

                await Task.Delay(WaitingTimeAfterCompleted, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка - {Exception}", ex);
                Logger.LogInformation("[{TaskName}] Задача завершилась с ошибкой: [{Time}]", TaskName, DateTimeOffset.Now);
            }
        }
    }

    public abstract Task RunAsync(CancellationToken cancellationToken = default);
}