using YandexDisk.API.Client.Contracts;

namespace YandexDisk.API.Client;

public class AuthHandler(ITokenStore tokenStore, ITokenService tokenService) : DelegatingHandler
{
    private readonly ITokenStore tokenStore = tokenStore;
    private readonly ITokenService tokenService = tokenService;

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", tokenStore.GetAccessToken());

        Console.WriteLine($"Запрос: {request.Method} {request.RequestUri}");
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var oAuthResponse = await tokenService.GetAccessTokenAsync(cancellationToken);

            if (oAuthResponse != null)
            {
                tokenStore.SetAccessToken(oAuthResponse.AccessToken);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", tokenStore.GetAccessToken());
            }

            response = await base.SendAsync(request, cancellationToken);
        }

        Console.WriteLine($"Ответ: {response.StatusCode}");
        return response;
    }
}