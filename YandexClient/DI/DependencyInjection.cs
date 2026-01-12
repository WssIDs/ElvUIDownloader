using Microsoft.Extensions.DependencyInjection;
using YandexDisk.API.Client.Contracts;
using YandexDisk.API.Client.Options;

namespace YandexDisk.API.Client.DI;

public static class ClientNames
{
    public static string AuthClient = "AuthClient";

    public static string DiskClient = "DiskClient";
}

public static class DependencyInjection
{
    public static IServiceCollection AddYandexDiskApi(this IServiceCollection services, Action<DiskAccessTokenOptions> clientOptions)
    {
        var options = new DiskAccessTokenOptions
        {
            AccessToken = "",
        };

        clientOptions.Invoke(options);

        services.Configure(clientOptions);

        services.AddTransient<AuthHandler>();

        services.AddHttpClient(ClientNames.AuthClient, client =>
        {
            client.BaseAddress = new Uri("https://oauth.yandex.ru");
        });

        services.AddHttpClient(ClientNames.DiskClient, client =>
        {
            client.BaseAddress = new Uri("https://cloud-api.yandex.net");
        })
         .AddHttpMessageHandler<AuthHandler>();

        services.AddSingleton<ITokenStore>(prov =>
        {
            var tokenStore = new TokenStore();

            if (options.AccessToken != null)
            {
                tokenStore.SetAccessToken(options.AccessToken);
            }

            return tokenStore;
        });
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IYandexDiskApi, YandexDiskApi>();


        return services;
    }
}