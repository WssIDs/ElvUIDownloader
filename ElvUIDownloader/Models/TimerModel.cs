using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public partial class TimerModel : ObservableObject
{
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    public partial TimeSpan Time { get; set; }
    
    [ObservableProperty]
    public partial bool IsRunning { get; set; }

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
        if (_timer.IsEnabled) return;
        _timer.Start();
        IsRunning = true;
    }

    public void Stop()
    {
        if (!_timer.IsEnabled) return;
        _timer.Stop();
        IsRunning = false;
    }
}