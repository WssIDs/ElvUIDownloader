using System.Text.RegularExpressions;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElvUIDownloader.Models;

namespace ElvUIDownloader.ViewModels;

public partial  class NewVersionViewModel : ViewModelBase
{
    /// <summary>
    /// 
    /// </summary>
    public NewVersionViewModel()
    {
        Title = "Что нового";
    }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string TextData { get; set; }

    [ObservableProperty]
    public partial TextModel Data { get; set; } = new()
    {
        Title = string.Empty
    };
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand(CanExecute = nameof(CanLoaded))]
    private async Task LoadedAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        const string filename = "readme.md";
        var fi = new FileInfo(filename);


        if (fi.Exists)
        {
            var lines = File.ReadLinesAsync(filename, cancellationToken);
            
            var isFirstLine = true;
            var isHeader = false;

            MessageModel? currentMessage = null;
            
            await foreach (var line in lines)
            {
                if (isFirstLine)
                {
                    Data.Title = line;
                    
                    isFirstLine = false;
                    continue;
                }

                if (string.IsNullOrEmpty(line))
                {
                    if (currentMessage != null)
                    {
                        Data.Messages.Add(currentMessage);
                    }

                    currentMessage = null;
                    
                    isHeader = true;
                    continue;
                }

                if (isHeader)
                {
                    var splitHeaders = line.Split(' ');

                    if (splitHeaders.Length == 3)
                    {
                        currentMessage = new MessageModel
                        {
                            Header = new HeaderModel
                            {
                                Version = $"{splitHeaders[0]} {splitHeaders[1]}",
                                Date = splitHeaders[2]
                            }
                        };
                    }
                    
                    isHeader = false;
                    continue;
                }

                // Регулярное выражение:
                // ^\[(.*?)\] — находит текст внутри квадратных скобок в начале строки
                // \s* — игнорирует пробелы после скобок
                // (.*)$ — забирает весь оставшийся текст до конца строки
                var match = MyRegex().Match(line);

                if (match.Success)
                {
                    currentMessage?.Lines.Add(new LineModel
                    {
                        Type = $"[{match.Groups[1].Value}]",
                        Text = match.Groups[2].Value
                    });
                }
            }
        }

        IsBusy = false;
    }

    private bool CanLoaded()
    {
        return !IsBusy;
    }

    [GeneratedRegex(@"^\[(.*?)\]\s*(.*)$")]
    private static partial Regex MyRegex();
}