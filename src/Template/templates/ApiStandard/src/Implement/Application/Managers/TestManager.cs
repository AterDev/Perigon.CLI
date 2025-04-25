using Entity.UserMod;
using EntityFramework.DBProvider;

namespace SharedModule.Managers;


public class TestManager(TenantDbContextFactory factory, ILogger<TestManager> logger)
    : ManagerBase<User>(factory, logger)
{

}
