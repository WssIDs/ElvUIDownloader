using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ElvUIDownloader.Utils;

namespace ElvUIDownloader.Models;

/// <summary>
/// 
/// </summary>
public class AppSettings
{
    private readonly RegistryService _registryService;
    
    public AppSettings(RegistryService registryService)
    {
        _registryService = registryService;
        UpdateAppInfo = new UpdateInfoModel
        {
            Interval = 180
        };
        UpdateAddonInfo = new UpdateInfoModel
        {
            Interval = 45
        };
    }
    
    public bool StartMinimize { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string CurrentProfile { get; set; } = "Default";

    /// <summary>
    /// 
    /// </summary>
    public UpdateInfoModel UpdateAppInfo { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public UpdateInfoModel UpdateAddonInfo { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string KeyToken { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<ProfileModel> Profiles { get; set; } = new();

    public async Task LoadProfilesAsync()
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),name);


        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var dirInfo = new DirectoryInfo(dir);

        var profileFiles = dirInfo.GetFiles().Where(f => Path.GetExtension(f.Name) == ".prof").ToList();

        foreach (var profileFile in profileFiles.Where(profileFile => File.Exists(profileFile.FullName)))
        {
            var data = await File.ReadAllTextAsync(profileFile.FullName);

            var profile = JsonSerializer.Deserialize<ProfileModel>(data);
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
                        Type = "retail",
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
    
    public async Task SaveProfileAsync(ProfileModel profile)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name);


        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        var data = JsonSerializer.Serialize(profile);
        await File.WriteAllTextAsync(Path.Combine(dir, $"{profile.Name}.prof"), data);
    }
}

public class UpdateInfoModel
{
    public UpdateInfoModel()
    {
        Type = "Minutes";
        Interval = 60;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public int Interval { get; set; }
}