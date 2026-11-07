namespace ElvUIDownloader.Utils;

public static class DirectoryInfoEx
{
    public static async Task CopyFilesRecursivelyAsync(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);

        await Task.Run(async() =>
        {
            //Now Create all the directories
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                await Task.Run(() => { Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath)); });
            }

            //Copy all the files & Replaces any files with the same name
            foreach (var newPath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                await Task.Run(() => { File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true); });
            }
        });
    }
}
