using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace YandexDisk.API.Client.Requests;

public class OAuthRequest
{
    [JsonPropertyName("grant_type")]
    public required string GrantType { get; set; }

    [JsonPropertyName("client_id")]
    public required string ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; set; }
}
