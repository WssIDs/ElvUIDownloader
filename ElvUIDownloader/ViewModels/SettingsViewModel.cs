using ElvUIDownloader.Commands;
using ElvUIDownloader.Models;
using ModernWpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace ElvUIDownloader.ViewModels;

public enum EApplicationTheme
{
    [Description("Светлая")]
    Light,

    [Description("Темная")]
    Dark,

    [Description("Системная")]
    System
}

/// <summary>
/// 
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        Title = "Настройки приложения";
        OkButton = "Сохранить";

        Types = new ObservableCollection<ETypeInterval>(Enum.GetValues<ETypeInterval>());

        Themes = new ObservableCollection<EApplicationTheme>(Enum.GetValues<EApplicationTheme>());

        //if (ThemeManager.Current.ApplicationTheme.HasValue)
        //{
        //    Theme = (EApplicationTheme)ThemeManager.Current.ApplicationTheme.Value;
        //}
        //else
        //{
        //    Theme = EApplicationTheme.System;
        //}

        //if (ThemeManager.Current.AccentColor.HasValue)
        //{
        //    UseSystemAccentColor = false;
        //    AccentColor = new SolidColorBrush(ThemeManager.Current.AccentColor.Value);
        //}
        //else
        //{
        //    UseSystemAccentColor = true;
        //    AccentColor = null;
        //}

            //var t = ThemeManager.Current.AccentColor;
    }

    private bool _startMinimized;

    public bool StartMinimized
    {
        get => _startMinimized;
        set => Set(ref _startMinimized, value);
    }

    private string _currentProfile = string.Empty;

    public string CurrentProfile
    {
        get => _currentProfile;
        set => Set(ref _currentProfile, value);
    }

    private UpdateInfoModel _updateAddonSettings = new();

    public UpdateInfoModel UpdateAddonSettings
    {
        get => _updateAddonSettings;
        set => Set(ref _updateAddonSettings, value);
    }

    private UpdateInfoModel _updateApplicationSettings = new();

    public UpdateInfoModel UpdateApplicationSettings
    {
        get => _updateApplicationSettings;
        set => Set(ref _updateApplicationSettings, value);
    }

    private ObservableCollection<ETypeInterval> _types = [];

    public ObservableCollection<ETypeInterval> Types
    {
        get => _types;
        set => Set(ref _types, value);
    }

    private EApplicationTheme _theme;

    public EApplicationTheme Theme
    {
        get => _theme;
        set
        {
            if (Set(ref _theme, value))
            {
                switch (_theme)
                {
                    case EApplicationTheme.Light:
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        break;
                    case EApplicationTheme.Dark:
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        break;
                    case EApplicationTheme.System:
                        ThemeManager.Current.ApplicationTheme = null;
                        break;
                }
            }
        }
    }

    private ObservableCollection<EApplicationTheme> _themes = [];

    public ObservableCollection<EApplicationTheme> Themes
    {
        get => _themes;
        set => Set(ref _themes, value);
    }

    private bool _useSystemAccentColor = true;

    public bool UseSystemAccentColor
    {
        get => _useSystemAccentColor;
        set
        {
            if (Set(ref _useSystemAccentColor, value))
            {
                if (_useSystemAccentColor)
                {
                    ThemeManager.Current.AccentColor = null;
                }
                else
                {
                    ThemeManager.Current.AccentColor = AccentColor?.Color;
                }
            }
        }
    }

    private SolidColorBrush? _accentColor;

    public SolidColorBrush? AccentColor
    {
        get => _accentColor;
        set
        {
            if (Set(ref _accentColor, value))
            {
                if (!UseSystemAccentColor)
                {
                    if (_accentColor != null)
                    {
                        ThemeManager.Current.AccentColor = _accentColor.Color;
                    }
                }
            }
        }
    }

    private ObservableCollection<SolidColorBrush> _accentColors =
    [
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0078D7")), // Синий
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF107C10")), // Зеленый
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD13438")), // Красный
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF8C00")), // Оранжевый
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5C2D91")), // Фиолетовый
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF038387")), // Бирюзовый
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE3008C")), // Розовый
        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7A7574")), // Серый
    ];

    public ObservableCollection<SolidColorBrush> AccentColors
    {
        get => _accentColors;
        set => Set(ref _accentColors, value);
    }

    private string _okButton = string.Empty;

    public string OkButton
    {
        get => _okButton;
        set => Set(ref _okButton, value);
    }

    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public AsyncCommand SaveProfileCommand => new(_ =>
    {
        DialogResult = true;
        return Task.CompletedTask;
    });
}