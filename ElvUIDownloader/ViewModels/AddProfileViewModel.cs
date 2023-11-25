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
        
        Types = new ObservableCollection<string>
        {
            "Retail",
            "Classic"
        };

        Profile = new ProfileModel();
    }
    
    private bool _isManualSelectDirectory;
    private List<string> _installedWows;
    private string? _selectedInstalledWow;
    private ProfileModel _profile;
    private string _title;
    private string _okButton;
    private ObservableCollection<string> _types;


    public ProfileModel Profile
    {
        get => _profile;
        set => SetField(ref _profile, value);
    }
    
    public bool IsManualSelectDirectory
    {
        get => _isManualSelectDirectory;
        set => SetField(ref _isManualSelectDirectory, value);
    }

    public ObservableCollection<string> Types
    {
        get => _types;
        set => SetField(ref _types, value);
    }

    public List<string> InstalledWows
    {
        get => _installedWows;
        set => SetField(ref _installedWows, value);
    }

    public string? SelectedInstalledWow
    {
        get => _selectedInstalledWow;
        set
        {
            if (SetField(ref _selectedInstalledWow, value))
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
        set => SetField(ref _okButton, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public AsyncCommand SelectFolderCommand =>
        new AsyncCommand(async (r) =>
        {
            var res = DialogService.OpenFolderDialog();

            if (res.Result)
            {
                Profile.InstallLocation = res.SelectedDirectory;
            }
        });
}