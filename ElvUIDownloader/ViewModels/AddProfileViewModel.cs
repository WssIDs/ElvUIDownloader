using System.Collections.ObjectModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElvUIDownloader.Models;

namespace ElvUIDownloader.ViewModels;

/// <summary>
/// 
/// </summary>
public partial class AddProfileViewModel : ViewModelBase
{
    public AddProfileViewModel()
    {
        Title = "Новый профиль";
        OkButton = "Добавить";

        Types =
        [
            EGameType.Retail,
            EGameType.Classic
        ];

        Profile = new ProfileModel();
    }

    [ObservableProperty]
    public partial ProfileModel Profile { get; set; }

    [ObservableProperty]
    public partial bool IsManualSelectDirectory { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<EGameType> Types { get; set; }

    [ObservableProperty]
    public partial List<string> InstalledWows { get; set; }

    [ObservableProperty]
    public partial string? SelectedInstalledWow { get; set; }

    partial void OnSelectedInstalledWowChanged(string? value)
    {
        if (!IsManualSelectDirectory)
        {
            Profile.InstallLocation = SelectedInstalledWow;
        }
    }

    [ObservableProperty]
    public partial string OkButton { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [RelayCommand] 
    private Task SelectFolderAsync(CancellationToken cancellationToken = default)
    {
        var res = DialogService.OpenFolderDialog();

        if (res.Result)
        {
            Profile.InstallLocation = res.SelectedDirectory;
        }

        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task SaveProfileAsync(CancellationToken cancellationToken = default)
    {
        DialogResult = true;
        return Task.CompletedTask;
    }
}