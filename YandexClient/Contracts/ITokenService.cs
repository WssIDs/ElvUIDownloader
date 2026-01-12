using System;
using System.Collections.Generic;
using System.Text;
using YandexDisk.API.Client.Responses;

namespace YandexDisk.API.Client.Contracts;

public interface ITokenService
{
    public Task<OAuthResponse?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    //public Task<OAuthCodeResponse?> GetAuthCodeAsync(CancellationToken cancellationToken = default);
}
