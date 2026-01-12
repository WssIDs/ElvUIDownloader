using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace YandexDisk.API.Client.Responses;

public class OAuthCodeResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public long ExpiredIn { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }
}
