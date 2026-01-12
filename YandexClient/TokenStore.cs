using YandexDisk.API.Client.Contracts;

namespace YandexDisk.API.Client;

/// <summary>
/// 
/// </summary>
public class TokenStore : ITokenStore
{
    private string? _accessToken;

    private DateTime _expiresAt;

    public string? GetAccessToken()
    {
        return _accessToken;
    }

    public void SetAccessToken(string accessToken, DateTime? expiresAt = null)
    {
        if (expiresAt != null)
        {
            expiresAt.Value.AddSeconds(30);
            _expiresAt = expiresAt.Value;
        }

        _accessToken = accessToken;
    }

    public bool IsTokenValid()
    {
        if(string.IsNullOrEmpty(_accessToken))
        {
            return false;
        }

        ///
        return true;
    }
}