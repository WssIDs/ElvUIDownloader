using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElvUIDownloader.Models;

public partial class TextModel : ObservableObject
{
    [ObservableProperty]
    public required partial string Title { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MessageModel> Messages { get; set; } = [];
}

public partial class LineModel : ObservableObject
{
    [ObservableProperty]
    public required partial string Type { get; set; }
    
    [ObservableProperty]
    public required partial string Text { get; set; }
}

public partial class HeaderModel : ObservableObject
{
    [ObservableProperty]
    public required partial string Version { get; set; }
    
    [ObservableProperty]
    public required partial string Date { get; set; }
}

public partial class MessageModel : ObservableObject
{
    [ObservableProperty] public required partial HeaderModel Header { get; set; }
    
    [ObservableProperty]
    public partial ObservableCollection<LineModel> Lines { get; set; } = [];
}