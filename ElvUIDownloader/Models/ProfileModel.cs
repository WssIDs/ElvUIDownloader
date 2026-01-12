using System.Text.Json.Serialization;
using ElvUIDownloader.Models.Abstraction;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public enum EGameType
{
    Retail,
    Classic
}

/// <summary>
/// 
/// </summary>
public class ProfileModel : ModelBase
{
    private string? _installLocation;
    private bool _isForceInstall;
    private DateTime? _date;
    private string? _version;
    private bool _isForceUpdate;
    private bool _isInstalled;

    private string _name = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string? InstallLocation
    {
        get => _installLocation;
        set => Set(ref _installLocation, value);
    }

    private EGameType _type;

    /// <summary>
    /// Тип игры
    /// </summary>
    public EGameType Type
    {
        get => _type;
        set => Set(ref _type, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsForceInstall
    {
        get => _isForceInstall;
        set => Set(ref _isForceInstall, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsForceUpdate
    {
        get => _isForceUpdate;
        set => Set(ref _isForceUpdate, value);
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public bool IsInstalled
    {
        get => _isInstalled;
        set => Set(ref _isInstalled, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? Date
    {
        get => _date;
        set => Set(ref _date, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string? Version
    {
        get => _version;
        set => Set(ref _version, value);
    }
}