using System.Text.Json.Serialization;

namespace YandexDisk.API.Client.Responses;

public class LinkResponse
{
    [JsonPropertyName("href")]
    public required string Href { get; set; }

    [JsonPropertyName("method")]
    public required string Method { get; set; }

    [JsonPropertyName("templated")]
    public bool Templated { get; set; }
}
