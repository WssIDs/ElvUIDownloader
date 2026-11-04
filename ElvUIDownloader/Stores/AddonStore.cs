using CommunityToolkit.Mvvm.ComponentModel;
using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Web;

namespace ElvUIDownloader.Stores;

public enum EAddonMode
{
    None,
    NeedInstall,
    NeedUpdate,
    Installed
}

/// <summary>
/// 
/// </summary>
public partial class AddonStore : ObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial TimerModel ElapsedTime { get; set; } = new(TimeSpan.FromSeconds(30));

    [ObservableProperty]
    public partial EAddonMode Mode { get; set; } = EAddonMode.None;

    [ObservableProperty]
    public partial DateTime InstallTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial ElvUiJsonModel? ElvUi { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string TextVersionInfo { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string PathToAddons { get; set; } = string.Empty;

    [ObservableProperty]
    public partial float Progress { get; set; }

    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    [ObservableProperty]
    public partial bool IsInstalling { get; set; }

    [ObservableProperty]
    public partial bool IsUpdateAddonTaskRunning { get; set; }

    [ObservableProperty]
    public partial string UpdateText { get; set; }

    [ObservableProperty]
    public partial string UpdateToolTipText { get; set; }
}