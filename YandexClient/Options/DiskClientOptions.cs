namespace YandexDisk.API.Client.Options;

public interface IDiskOptions
{
}

public interface IDiskAccessTokenOptions : IDiskOptions
{
    public string AccessToken { get; set; }
}

public interface IDiskClientOptions : IDiskOptions
{
    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}

public class DiskAccessTokenOptions : IDiskAccessTokenOptions
{
    public required string AccessToken { get; set; }
}

public class DiskClientOptions : IDiskClientOptions
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }
}