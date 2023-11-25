namespace ElvUIDownloader.Utils;

public static class DirectoryInfoEx
{
    public static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
    
    public static async Task CopyFilesRecursivelyAsync(string sourcePath, string targetPath)
    {
        await Task.Run(async() =>
        {
            //Now Create all of the directories
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