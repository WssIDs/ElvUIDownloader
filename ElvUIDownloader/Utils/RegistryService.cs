using Microsoft.Win32;

namespace ElvUIDownloader.Utils;

public class RegistryService
{
    public async Task<List<DirectoryInfo>> GetWows()
    {
        const string registryKey32 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        const string registryKey64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";

        var installedWows = GetInstalledWows(registryKey32).ToList();
        installedWows.AddRange(GetInstalledWows(registryKey64).ToList());

        return installedWows;
    }
    
    private HashSet<DirectoryInfo> GetInstalledWows(string keyPath)
    {
        var hashSet = new HashSet<DirectoryInfo>();
            
        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
        if (key == null) return hashSet;
        foreach (var name in key.GetSubKeyNames())
        {
            using var subKey = key.OpenSubKey(name);
            if (subKey?.GetValue("DisplayName") == null) continue;
                
            var displayName = subKey.GetValue("DisplayName")!.ToString();

            if (string.IsNullOrEmpty(displayName)) continue;
            if (!displayName.ToLower().Contains("World of Warcraft".ToLower()) &&
                !displayName.ToLower().Contains("WoW".ToLower())) continue;
            var installLocation = subKey.GetValue("InstallLocation")!.ToString();

            if (!string.IsNullOrEmpty(installLocation))
            {
                var di = new DirectoryInfo(installLocation);

                if (di.Exists)
                {
                    hashSet.Add(di);
                }
            }
        }

        return hashSet;
    }
}