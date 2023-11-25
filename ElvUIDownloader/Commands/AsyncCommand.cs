using System.Windows.Input;

namespace ElvUIDownloader.Commands;

/// <summary>
/// 
/// </summary>
public interface IAsyncCommand : ICommand
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task ExecuteAsync(object? param);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool CanExecute(object? param);

    /// <summary>
    /// 
    /// </summary>
    void RaiseCanExecuteChanged();
}

/// <summary>
/// 
/// </summary>
public class AsyncCommand : IAsyncCommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested += value;
    }
    
    private readonly Func<object?,Task> _execute;
    private readonly Func<object?,bool>? _canExecute;
    private readonly IErrorHandler? _errorHandler;

    public AsyncCommand(
        Func<object?,Task> execute,
        Func<object?, bool>? canExecute = null,
        IErrorHandler? errorHandler = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _errorHandler = errorHandler;
    }

    public bool CanExecute(object? param = null)
    {
        return _canExecute?.Invoke(param) ?? true;
    }

    public async Task ExecuteAsync(object? param = null)
    {
        await _execute.Invoke(param);
        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute(parameter);
    }

    void ICommand.Execute(object? parameter)
    {
        ExecuteAsync(parameter).FireAndForgetSafeAsync(_errorHandler);
    }
    #endregion
}