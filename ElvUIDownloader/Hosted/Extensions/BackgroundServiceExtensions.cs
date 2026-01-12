using ElvUIDownloader.Hosted.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ElvUIDownloader.Hosted.Extensions;

public static class BackgroundServiceExtensions
{
    public static IServiceCollection AddBackgroundService<T>(this IServiceCollection services) where T : BaseBackgroundService
    {
        services.AddHostedService<T>();

        return services;
    }
}