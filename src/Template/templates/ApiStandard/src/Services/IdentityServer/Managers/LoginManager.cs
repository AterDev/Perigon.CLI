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
            return new LoginResult { IsSuccess = false, ErrorMessage = "用户不存在" };
        }
        if (!user.AccountRoles.Any(ar => ar.Role.Name == ConstVal.SuperAdmin))
        {
            return new LoginResult
            {
                IsSuccess = false,
                ErrorMessage = "无权限，仅超级管理员可登录",
            };
        }
        var hash = HashCrypto.GeneratePwd(password, user.HashSalt);
        if (user.HashPassword != hash)
        {
            return new LoginResult { IsSuccess = false, ErrorMessage = "密码错误" };
        }
        return new LoginResult { IsSuccess = true };
    }
}
