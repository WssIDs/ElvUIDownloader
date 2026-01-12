using System.Net.Http.Json;
using System.Web;
using YandexDisk.API.Client.Contracts;
using YandexDisk.API.Client.DI;
using YandexDisk.API.Client.Responses;

namespace YandexDisk.API.Client;

public class YandexDiskApi(IHttpClientFactory httpClientFactory) : IYandexDiskApi
{
	private const string ApiUrl = "https://cloud-api.yandex.net";

	private const string ApiVersion = "v1";

	private const string ApiService = "disk";

	private const string ApiRelativePath = "resources";

	private const string ApiRelativeTrashPath = "trash/resources";

	private IHttpClientFactory _httpClientFactory = httpClientFactory;

    private string GetRelativeUrl(string action)
	{
		return Path.Combine(ApiVersion, ApiService, ApiRelativePath, action);
	}

	private HttpClient GetClient()
	{
		var httpClient = _httpClientFactory.CreateClient(ClientNames.DiskClient);

        return httpClient;
	}

	public async Task<LinkResponse?> SendGetRequestAsync(string action, string filePath, CancellationToken cancellationToken = default)
	{
        using var httpClient = GetClient();

        using var response = await httpClient.GetAsync($"{GetRelativeUrl(action)}?path={filePath}", cancellationToken);

		response.EnsureSuccessStatusCode();

        // Читаем ответ
        var linkResponse = await response.Content.ReadFromJsonAsync<LinkResponse>(cancellationToken);

        return linkResponse;
	}

	public async Task<Stream?> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
	{
		// Читаем ответ
		var linkResponse = await SendGetRequestAsync("download", filePath, cancellationToken);


        if (linkResponse == null) return null;

		Stream? result = null;

		if(linkResponse.Method == "GET")
		{
            using var httpClient = GetClient();

            var streamResponse = await httpClient.GetAsync(linkResponse.Href, cancellationToken);

			if(streamResponse == null) return null;

			if (!streamResponse.IsSuccessStatusCode)
			{
				return null;
			}

			result = streamResponse.Content.ReadAsStream(cancellationToken);

        }

		return result;
    }
}