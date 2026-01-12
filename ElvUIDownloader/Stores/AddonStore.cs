using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Abstraction;
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
public class AddonStore : ModelBase
{
    /// <summary>
    /// 
    /// </summary>
    public TimerModel ElapsedTime { get; set; } = new(TimeSpan.FromSeconds(30));

    private EAddonMode _mode = EAddonMode.None;

    public EAddonMode Mode
    {
        get => _mode;
        set => Set(ref _mode, value);
    }

    public DateTime _installTime;

    public DateTime InstallTime
    {
        get => _installTime;
        set => Set(ref _installTime, value);
    }

    private ElvUiModel? _elvUi;

    /// <summary>
    /// 
    /// </summary>
    public ElvUiModel? ElvUi
    {
        get => _elvUi;
        set => Set(ref _elvUi, value);
    }

    private string _textVersionInfo = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string TextVersionInfo
    {
        get => _textVersionInfo;
        set => Set(ref _textVersionInfo, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string PathToAddons { get; set; } = string.Empty;

    private float _progress;

    public float Progress
    {
        get => _progress;
        set => Set(ref _progress, value);
    }

    private bool _isDownloading;

    public bool IsDownloading
    {
        get => _isDownloading;
        set => Set(ref _isDownloading, value);
    }

    private bool _isInstalling;

    public bool IsInstalling
    {
        get => _isInstalling;
        set => Set(ref _isInstalling, value);
    }

    private bool _isUpdateAddonTaskRunning;

    public bool IsUpdateAddonTaskRunning
    {
        get => _isUpdateAddonTaskRunning;
        set => Set(ref _isUpdateAddonTaskRunning, value);
    }

    private string _updateText = string.Empty;

    public string UpdateText
    {
        get => _updateText;
        set => Set(ref _updateText, value);
    }

    private string _updateToolTipText = string.Empty;

    public string UpdateToolTipText
    {
        get => _updateToolTipText;
        set => Set(ref _updateToolTipText, value);
    }
}