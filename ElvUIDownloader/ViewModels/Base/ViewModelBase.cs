using CommunityToolkit.Mvvm.ComponentModel;
using ElvUIDownloader.DI;
using Microsoft.Extensions.DependencyInjection;

namespace ElvUIDownloader.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject
{
    protected readonly DialogService DialogService = SimpleIoC.ServiceProvider.GetRequiredService<DialogService>();

    [ObservableProperty]
	public partial bool? DialogResult { get; set; }
}