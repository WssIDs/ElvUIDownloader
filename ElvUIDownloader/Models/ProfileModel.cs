using System.Text.Json.Serialization;
using ElvUIDownloader.Models.Abstraction;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public class ProfileModel : ModelBase
{
    private string _name;
    private string? _installLocation;
    private string _type;
    private bool _isForceInstall;
    private DateTime? _date;
    private string? _version;
    private bool _isForceUpdate;
    private bool _isInstalled;

    /// <summary>
    /// 
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string? InstallLocation
    {
        get => _installLocation;
        set => SetField(ref _installLocation, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsForceInstall
    {
        get => _isForceInstall;
        set => SetField(ref _isForceInstall, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsForceUpdate
    {
        get => _isForceUpdate;
        set => SetField(ref _isForceUpdate, value);
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public bool IsInstalled
    {
        get => _isInstalled;
        set => SetField(ref _isInstalled, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? Date
    {
        get => _date;
        set => SetField(ref _date, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public string? Version
    {
        get => _version;
        set => SetField(ref _version, value);
    }
}