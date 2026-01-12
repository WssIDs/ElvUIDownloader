namespace YandexDisk.API.Client.Contracts;

public interface ITokenStore
{
    public string? GetAccessToken();

    public void SetAccessToken(string accessToken, DateTime? expiresAt = null);

    public bool IsTokenValid();
}
