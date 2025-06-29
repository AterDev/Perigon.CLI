using Ater.Common.Utils;
using IdentityServer.Definition.Entity;
using IdentityServer.Definition.Share.AccountDtos;

namespace IdentityServer.Managers;

public class LoginManager
{
    public LoginResult ValidateLogin(Account? user, string password)
    {
        if (user is null)
        {
            return new LoginResult { IsSuccess = false, ErrorMessage = "User not found" };
        }
        if (!user.AccountRoles.Any(ar => ar.Role.Name == ConstVal.SuperAdmin))
        {
            return new LoginResult { IsSuccess = false, ErrorMessage = "Forbidden, no access" };
        }
        var hash = HashCrypto.GeneratePwd(password, user.HashSalt);
        if (user.HashPassword != hash)
        {
            return new LoginResult { IsSuccess = false, ErrorMessage = "Password incorrect" };
        }
        return new LoginResult { IsSuccess = true };
    }
}
