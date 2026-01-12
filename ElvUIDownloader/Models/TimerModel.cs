using ElvUIDownloader.Models.Abstraction;
using System.Windows.Threading;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public class TimerModel : ModelBase
{
    private readonly DispatcherTimer _timer;

    private TimeSpan _time;

    public TimeSpan Time
    {
        get => _time;
        set => Set(ref _time, value);
    }

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set => Set(ref _isRunning, value);
    }

    public TimerModel(TimeSpan elapsedTime)
    {
        Time = elapsedTime;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
    }

    ~TimerModel()
    {
        IsRunning = false;
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (Time.TotalSeconds > 0)
        {
            Time = Time.Add(TimeSpan.FromSeconds(-1));
            //Debug.WriteLine($"Время: {Time}");
        }
        else
        {
            _timer.Stop();
            IsRunning = false;
            //Debug.WriteLine($"Таймер остановлен");
        }
    }

    public void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
            IsRunning = true;
        }
    }

    public void Stop()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            IsRunning = false;
        }
    }
}