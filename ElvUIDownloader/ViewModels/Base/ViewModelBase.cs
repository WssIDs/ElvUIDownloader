using ElvUIDownloader.DI;
using ElvUIDownloader.Models.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace ElvUIDownloader.ViewModels.Base;

public abstract class ViewModelBase : ModelBase
{
    protected readonly DialogService DialogService = SimpleIoC.GetInstance().Provider.GetRequiredService<DialogService>();
}