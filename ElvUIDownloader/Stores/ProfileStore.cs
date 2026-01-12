using ElvUIDownloader.Models;
using ElvUIDownloader.Models.Abstraction;

namespace ElvUIDownloader.Stores;

public class ProfileStore : ModelBase
{
    private readonly AppSettings _appSettings;

    public ProfileStore(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    private ProfileModel? _currentProfile;

    /// <summary>
    /// 
    /// </summary>
    public ProfileModel? CurrentProfile
    {
        get => _currentProfile;
        set
        {
            if(Set(ref _currentProfile, value))
            {
                if(_currentProfile != null)
                {
                    _appSettings.CurrentProfile = _currentProfile.Name;
                }
            }
        }
    }
}