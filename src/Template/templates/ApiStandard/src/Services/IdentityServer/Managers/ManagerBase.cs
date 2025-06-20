namespace IdentityServer.Managers;

public class ManagerBase(ILogger logger)
{
    private readonly ILogger _logger = logger;

    protected string? ErrorMessage { get; set; }
}
