using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Abstraction;
using System.Diagnostics;

namespace ElvUIDownloader.Stores;

/// <summary>
/// 
/// </summary>
public class ApplicationStore : ModelBase
{
    public ApplicationStore()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        if (executingAssembly != null)
        {
            Name = executingAssembly.GetName().Name ?? string.Empty;

            var currentAppVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).FileVersion ?? string.Empty;

            if (string.IsNullOrEmpty(currentAppVersion))
            {
                currentAppVersion = "1.0.0.0";
            }

            if(Version.TryParse(currentAppVersion, out var version))
            {
                Version = version;
            }

            var fi = new FileInfo(executingAssembly.Location);

            if (fi.Exists)
            {
                InstallTime = fi.LastWriteTime;
            }

            RemoteFileVersionFilename = $"{Name}/fileinfo.json";

            var ext = ".exe";
            var setupName = $"Setup";
            var setupFilename = $"{Name}_{setupName}{ext}";

            RemoteSetupFilename = $"{Name}/{setupFilename}";

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name!);
            var setupDirectoryName = "Setups";

            var setupDirectory = new DirectoryInfo(Path.Combine(directory, setupDirectoryName));

            if (!setupDirectory.Exists)
            {
                setupDirectory.Create();
            }

            var localSetupFilename = Path.Combine(setupDirectory.FullName, setupFilename);

            LocalSetupFilename = new FileInfo(localSetupFilename);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public Version Version { get; set; } = new Version(1,0,0,0);

    /// <summary>
    /// 
    /// </summary>
    public TimerModel ElapsedTime { get; set; } = new(TimeSpan.FromSeconds(30));

    /// <summary>
    /// 
    /// </summary>
    public bool IsNeedUpdate { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public DateTime InstallTime { get; set; }

    public string RemoteFileVersionFilename { get; set; } = string.Empty;

    public string RemoteSetupFilename { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public FileInfo LocalSetupFilename { get; set; } = null!;
}