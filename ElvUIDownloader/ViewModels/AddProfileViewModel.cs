using System.Collections.ObjectModel;
using ElvUIDownloader.Commands;
using ElvUIDownloader.Models;

namespace ElvUIDownloader.ViewModels;

/// <summary>
/// 
/// </summary>
public class AddProfileViewModel : ViewModelBase
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

    private bool _isManualSelectDirectory;
    private List<string> _installedWows = [];
    private string? _selectedInstalledWow;
    private ProfileModel _profile = null!;
    private string _title = null!;
    private string _okButton = null!;
    private ObservableCollection<EGameType> _types = [];


    public ProfileModel Profile
    {
        get => _profile;
        set => Set(ref _profile, value);
    }

    public bool IsManualSelectDirectory
    {
        get => _isManualSelectDirectory;
        set => Set(ref _isManualSelectDirectory, value);
    }

    public ObservableCollection<EGameType> Types
    {
        get => _types;
        set => Set(ref _types, value);
    }

    public List<string> InstalledWows
    {
        get => _installedWows;
        set => Set(ref _installedWows, value);
    }

    public string? SelectedInstalledWow
    {
        get => _selectedInstalledWow;
        set
        {
            if (Set(ref _selectedInstalledWow, value))
            {
                if (!IsManualSelectDirectory)
                {
                    Profile.InstallLocation = SelectedInstalledWow;
                }
            }
        }
    }

    public string OkButton
    {
        get => _okButton;
        set => Set(ref _okButton, value);
    }

    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public AsyncCommand SelectFolderCommand =>
        new(_ =>
        {
            var res = DialogService.OpenFolderDialog();

            if (res.Result)
            {
                Profile.InstallLocation = res.SelectedDirectory;
            }

            return Task.CompletedTask;
        });

    public AsyncCommand SaveProfileCommand => new(_ =>
    {
        DialogResult = true;
        return Task.CompletedTask;
    });
}