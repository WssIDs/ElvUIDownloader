namespace ElvUIDownloader.Commands;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}