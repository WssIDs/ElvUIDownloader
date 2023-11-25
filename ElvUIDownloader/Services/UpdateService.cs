using System.Diagnostics;
using System.Threading;
using ElvUIDownloader.Models;
using YandexDisk.Client.Clients;
using YandexDisk.Client.Http;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ElvUIDownloader.Services;

public class UpdateService
{
    private readonly AppSettings _appSettings;

    public UpdateService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task RunAsync(CancellationToken token = default)
    {
        try
        {
            if(token.IsCancellationRequested) return;
            
            var api = new DiskHttpApi(_appSettings.KeyToken);
            var executingAssembly = Assembly.GetExecutingAssembly();

            var stream =
                await api.Files.DownloadFileAsync("disk:/" + $"{executingAssembly.GetName().Name}/" + "fileinfo.json",
                    cancellationToken: token);

            var data = await JsonSerializer.DeserializeAsync<AppVersionModel>(stream, cancellationToken: token);

            var currentAppVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).ProductVersion;

            if (data != null && currentAppVersion != null)
            {
                if (Version.Parse(data.FileVersion) > Version.Parse(currentAppVersion))
                {
                    var setupStream = await api.Files.DownloadFileAsync(
                        "disk:/" + $"{executingAssembly.GetName().Name}/" +
                        $"{executingAssembly.GetName().Name}_Setup.exe", cancellationToken: token);

                    var filename = Path.Combine(Path.GetTempPath(), $"{executingAssembly.GetName().Name}_Setup.exe");

                    await using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        await setupStream.CopyToAsync(fs, token);
                    }

                    await Task.Run(() =>
                    {
                        var process = new Process();
                        const string silent = "/VERYSILENT";
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = filename,
                            Arguments = $"{silent}"
                        };

                        process.StartInfo = processInfo;
                        process.Start();
                    }, token);
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}