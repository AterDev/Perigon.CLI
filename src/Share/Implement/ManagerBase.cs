namespace Share.Implement;

/// <summary>
/// Base manager class without dbContext
/// </summary>
/// <param name="logger">Logger instance</param>
public abstract class ManagerBase(ILogger logger)
{
    protected ILogger _logger = logger;
}