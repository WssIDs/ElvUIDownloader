using ElvUIDownloader.Models;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElvUIDownloader.Utils;

#pragma warning disable WPF0001 // ThemeMode помечен как experimental

namespace ElvUIDownloader.ViewModels;

/// <summary>
/// 
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        Title = "Настройки приложения";
        OkButton = "Сохранить";

        Types = [.. Enum.GetValues<ETypeInterval>()];
        Themes = [.. Enum.GetValues<EApplicationTheme>()];
    }
    
    [ObservableProperty]
    public partial bool StartMinimized { get; set; }
    
    [ObservableProperty]
    public partial string CurrentProfile { get; set; }
    
    [ObservableProperty]
    public partial UpdateInfoModel UpdateAddonSettings { get; set; } = new();
    
    [ObservableProperty]
    public partial UpdateInfoModel UpdateApplicationSettings { get; set; } = new();
    
    [ObservableProperty]
    public partial ObservableCollection<ETypeInterval> Types { get; set; }
    
    [ObservableProperty]
    public partial EApplicationTheme Theme { get; set; }

    partial void OnThemeChanged(EApplicationTheme value)
    {
        AccentThemeManager.SetTheme(Theme, UseSystemAccentColor ? null : AccentColor?.Color);
    }
    
    [ObservableProperty]
    public partial ObservableCollection<EApplicationTheme> Themes { get; set; }
    
    [ObservableProperty]
    public partial bool UseSystemAccentColor { get; set; }

    partial void OnUseSystemAccentColorChanged(bool value)
    {
        if (!UseSystemAccentColor)
        {
            AccentThemeManager.SetTheme(Theme, AccentColor?.Color);
        }
        else
        {
            AccentThemeManager.SetTheme(Theme);
        }
    }
    
    [ObservableProperty]
    public partial SolidColorBrush? AccentColor { get; set; }

    partial void OnAccentColorChanged(SolidColorBrush? value)
    {
        if (!UseSystemAccentColor)
        {
            AccentThemeManager.SetTheme(Theme, AccentColor?.Color);
        }
        else
        {
            AccentThemeManager.SetTheme(Theme);
        }
    }

    [ObservableProperty]
    public partial ObservableCollection<SolidColorBrush> AccentColors { get; set; } =
    [
        new ((Color)ColorConverter.ConvertFromString("#FF0078D7")), // Синий
        new ((Color)ColorConverter.ConvertFromString("#FF107C10")), // Зеленый
        new ((Color)ColorConverter.ConvertFromString("#FFD13438")), // Красный
        new ((Color)ColorConverter.ConvertFromString("#FFFF8C00")), // Оранжевый
        new ((Color)ColorConverter.ConvertFromString("#FF5C2D91")), // Фиолетовый
        new ((Color)ColorConverter.ConvertFromString("#FF038387")), // Бирюзовый
        new ((Color)ColorConverter.ConvertFromString("#FFE3008C")), // Розовый
        new ((Color)ColorConverter.ConvertFromString("#FF7A7574")), // Серый
    ];
    
    [ObservableProperty]
    public partial string OkButton { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [RelayCommand]
    private Task SaveProfileAsync(CancellationToken cancellationToken = default)
    {
        DialogResult = true;
        return Task.CompletedTask;
    }
}