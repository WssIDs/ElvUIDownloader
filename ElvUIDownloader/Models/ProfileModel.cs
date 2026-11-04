using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

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
public partial class ProfileModel : ObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string? InstallLocation { get; set; }

    /// <summary>
    /// Тип игры
    /// </summary>
    [ObservableProperty]
    public partial EGameType Type { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial bool IsForceInstall { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial bool IsForceUpdate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    [ObservableProperty]
    public partial bool IsInstalled { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial DateTime? Date { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial string? Version { get; set; }
}