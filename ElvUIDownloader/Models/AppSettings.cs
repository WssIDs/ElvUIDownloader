using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Media;
using ElvUIDownloader.Models.Abstraction;
using ElvUIDownloader.Utils;
using ElvUIDownloader.ViewModels;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public class AppSettings : ModelBase
{
    private readonly RegistryService _registryService;
    
    public AppSettings(RegistryService registryService)
    {
        _registryService = registryService;
        //UpdateAppInfo = new UpdateInfoModel
        //{
        //    Interval = 180
        //};
        //UpdateAddonInfo = new UpdateInfoModel
        //{
        //    Interval = 45
        //};
    }
    
    public bool StartMinimize { get; set; }

    public EApplicationTheme Theme { get; set; } = EApplicationTheme.System;

    /// <summary>
    /// 
    /// </summary>
    public bool UseSystemAccentColor { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public string? AccentColor { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public string CurrentProfile { get; set; } = "Default";

    /// <summary>
    /// 
    /// </summary>
    public UpdateInfoModel UpdateAppInfo { get; set; } = new();


    private UpdateInfoModel _updateAddonInfo = new();

    /// <summary>
    /// 
    /// </summary>
    public UpdateInfoModel UpdateAddonInfo
    {
        get => _updateAddonInfo;
        set => Set(ref _updateAddonInfo, value);
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<ProfileModel> Profiles { get; set; } = [];

    public async Task LoadProfilesAsync()
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name!);


        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var dirInfo = new DirectoryInfo(dir);

        var profileFiles = dirInfo.GetFiles().Where(f => Path.GetExtension(f.Name) == ".prof").ToList();

        foreach (var profileFile in profileFiles.Where(profileFile => File.Exists(profileFile.FullName)))
        {
            var data = await File.ReadAllTextAsync(profileFile.FullName);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            var profile = JsonSerializer.Deserialize<ProfileModel>(data, options);
            if (profile != null)
            {
                Profiles.Add(profile);
            }
        }

        if (Profiles.Count == 0)
        {
            var wows = await _registryService.GetWows();
            
            // create Default Profile;

            var wow = wows.FirstOrDefault();

            if (wow != null)
            {
                if (wow.Exists)
                {
                    var newProfile = new ProfileModel
                    {
                        Name = CurrentProfile,
                        Type = EGameType.Retail,
                        InstallLocation = wow.FullName,
                        IsForceInstall = true,
                        IsForceUpdate = true
                    };
                    
                    Profiles.Add(newProfile);
                    await SaveProfileAsync(newProfile);
                }
            }
        }
        
        if (string.IsNullOrEmpty(CurrentProfile))
        {
            var curProfile = Profiles.FirstOrDefault();
            if (curProfile != null)
            {
                CurrentProfile = curProfile.Name;
            }
        }
        else
        {
            var curProfile = Profiles.FirstOrDefault(p => p.Name == CurrentProfile);
            if (curProfile == null)
            {
                curProfile = Profiles.FirstOrDefault();
                if (curProfile != null)
                {
                    CurrentProfile = curProfile.Name;
                }
            }
        }
    }
    
    public async Task SaveProfileAsync(ProfileModel profile, CancellationToken cancellationToken = default)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name!);


        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var data = JsonSerializer.Serialize(profile, options);
        await File.WriteAllTextAsync(Path.Combine(dir, $"{profile.Name}.prof"), data, cancellationToken);
    }
}

public class UpdateInfoModel : ViewModelBase
{
    public UpdateInfoModel()
    {
        Type = ETypeInterval.Minutes;
        Interval = 60;
    }

    private ETypeInterval _type;

    /// <summary>
    /// 
    /// </summary>
    public ETypeInterval Type
    {
        get => _type;
        set => Set(ref _type, value);
    }
    
    private int _interval;

    /// <summary>
    /// 
    /// </summary>
    public int Interval
    {
        get => _interval;
        set => Set(ref _interval, value);
    }

    public TimeSpan GetTimeInterval()
    {
        var time = Type switch
        {
            //ETypeInterval.Milliseconds => TimeSpan.FromMilliseconds(Interval),
            ETypeInterval.Minutes => TimeSpan.FromMinutes(Interval),
            ETypeInterval.Seconds => TimeSpan.FromSeconds(Interval),
            ETypeInterval.Hours => TimeSpan.FromHours(Interval),
            ETypeInterval.Days => TimeSpan.FromDays(Interval),
            _ => TimeSpan.FromMinutes(Interval)
        };

        return time;
    }
}

public enum ETypeInterval
{
    //[Description("Милисекунды")]
    //Milliseconds,

    [Description("Секунды")]
    Seconds,

    [Description("Минуты")]
    Minutes,

    [Description("Часы")]
    Hours,

    [Description("Дни")]
    Days
}