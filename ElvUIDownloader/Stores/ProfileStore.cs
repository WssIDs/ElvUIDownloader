using CommunityToolkit.Mvvm.ComponentModel;
using ElvUIDownloader.Models;

namespace ElvUIDownloader.Stores;

public partial class ProfileStore(AppSettings appSettings) : ObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial ProfileModel? CurrentProfile { get; set; }

    partial void OnCurrentProfileChanged(ProfileModel? value)
    {
        if (CurrentProfile != null)
        {
            appSettings.CurrentProfile = CurrentProfile.Name;
        }
    }
}