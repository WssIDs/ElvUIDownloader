using ElvUIDownloader.Commands;

namespace ElvUIDownloader.ViewModels;

public class NewVersionViewModel : ViewModelBase
{
    private string _title = string.Empty;
    private bool _isBusy;

    private string _textData = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public NewVersionViewModel()
    {
        Title = "Что нового";
    }

    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => Set(ref _isBusy, value);
    }

    public string TextData
    {
        get => _textData;
        set => Set(ref _textData, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public AsyncCommand LoadedCommand =>
        new(async _ =>
        {
            IsBusy = true;

            const string filename = "readme.md";
            var fi = new FileInfo(filename);

            if (fi.Exists)
            {
                TextData = await File.ReadAllTextAsync(filename);
            }
            
            IsBusy = false;
        }, (r) => !IsBusy);
}