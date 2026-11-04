using ElvUIDownloader.Models;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElvUIDownloader.Stores;

/// <summary>
/// 
/// </summary>
public class ApplicationStore : ObservableObject
{
    public ApplicationStore()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        Name = executingAssembly.GetName().Name ?? string.Empty;

        var currentAppVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).FileVersion ?? string.Empty;

        if (string.IsNullOrEmpty(currentAppVersion))
        {
            currentAppVersion = "1.0.0.0";
        }

        if (Version.TryParse(currentAppVersion, out var version))
        {
            Version = version;
        }

        var fi = new FileInfo(executingAssembly.Location);

        if (fi.Exists)
        {
            InstallTime = fi.LastWriteTime;
        }

        RemoteFileVersionFilename = $"{Name}/fileinfo.json";

        const string ext = ".exe";
        const string setupName = "Setup";
        var setupFilename = $"{Name}_{setupName}{ext}";

        RemoteSetupFilename = $"{Name}/{setupFilename}";

        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name);
        const string setupDirectoryName = "Setups";

        var setupDirectory = new DirectoryInfo(Path.Combine(directory, setupDirectoryName));

        if (!setupDirectory.Exists)
        {
            setupDirectory.Create();
        }

        var localSetupFilename = Path.Combine(setupDirectory.FullName, setupFilename);

        LocalSetupFilename = new FileInfo(localSetupFilename);
    }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Version Version { get; set; } = new(1,0,0,0);

    /// <summary>
    /// 
    /// </summary>
    public TimerModel ElapsedTime { get; set; } = new(TimeSpan.FromSeconds(30));

    /// <summary>
    /// 
    /// </summary>
    public bool IsNeedUpdate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime InstallTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RemoteFileVersionFilename { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RemoteSetupFilename { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public FileInfo LocalSetupFilename { get; set; }
}