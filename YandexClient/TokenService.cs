using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using YandexDisk.API.Client.Contracts;
using YandexDisk.API.Client.DI;
using YandexDisk.API.Client.Options;
using YandexDisk.API.Client.Requests;
using YandexDisk.API.Client.Responses;

namespace YandexDisk.API.Client;

public class TokenService : ITokenService
{
    private readonly DiskClientOptions _diskClientOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenService(IOptions<DiskClientOptions> options, IHttpClientFactory httpClientFactory)
    {
        _diskClientOptions = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OAuthResponse?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(ClientNames.AuthClient);

        var oAuthRequest = new OAuthRequest
        {
            GrantType = "client_credentials",
            ClientId = _diskClientOptions.ClientId,
            ClientSecret = _diskClientOptions.ClientSecret
        };

        using var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", oAuthRequest.GrantType),
            new KeyValuePair<string, string>("client_id", oAuthRequest.ClientId),
            new KeyValuePair<string, string>("client_secret", oAuthRequest.ClientSecret)
        ]);

        var response = await client.PostAsync("/token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OAuthResponse>();

        if (result == null) return null;

        //AccessToken = "y0_AgAAAAAYlmlrAArc2QAAAADyqLtfppXHAPaNQySw1s5gN0ZovfovCq8";
        //ExpiresAt = DateTime.UtcNow.AddSeconds(result.ExpiredIn - 30); // небольшой запас

        return result;
    }

    //public async Task<OAuthCodeResponse?> GetAuthCodeAsync(CancellationToken cancellationToken = default)
    //{
    //    using var client = _httpClientFactory.CreateClient("AuthClient");

    //    var response = await client.GetAsync($"/authorize?response_type=code&client_id={_diskClientOptions.ClientId}", cancellationToken);
    //    //response.EnsureSuccessStatusCode();

    //    return new();
    //}
}