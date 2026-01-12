using ElvUIDownloader.DI;
using ElvUIDownloader.Models.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace ElvUIDownloader.ViewModels.Base;

public abstract class ViewModelBase : ModelBase
{
    protected readonly DialogService DialogService = SimpleIoC.ServiceProvider.GetRequiredService<DialogService>();

	private bool? _dialogResut;

	public bool? DialogResult
    {
        get => _dialogResut;
        set => Set(ref _dialogResut, value);
    }
}