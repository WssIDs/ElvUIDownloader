using System;
using System.Collections.Generic;
using System.Text;

namespace YandexDisk.API.Client.Contracts;

public interface IYandexDiskApi
{
    public Task<Stream?> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);
}
