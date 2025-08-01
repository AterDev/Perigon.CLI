using Definition.Share.Models.UserDtos;

namespace Modules.Manager;

/// <summary>
/// 用户账户
/// </summary>
public class UserManager(
    DataAccessContext<User> dataContext,
    ILogger<UserManager> logger,
    IUserContext userContext
) : ManagerBase<User, UserUpdateDto, UserFilterDto, UserItemDto>(dataContext, logger)
{
    private readonly IUserContext _userContext = userContext;

    /// <summary>
    /// 更新密码
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<bool> ChangePasswordAsync(User user, string newPassword)
    {
        user.PasswordSalt = HashCrypto.BuildSalt();
        user.PasswordHash = HashCrypto.GeneratePwd(newPassword, user.PasswordSalt);
        Command.Update(user);
        return await Command.SaveChangesAsync() > 0;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        var user = new User { UserName = dto.UserName, PasswordSalt = HashCrypto.BuildSalt() };
        user.PasswordHash = HashCrypto.GeneratePwd(dto.Password, user.PasswordSalt);
        if (dto.Email != null)
        {
            user.Email = dto.Email;
        }
        await AddAsync(user);
        return user;
    }

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<User> CreateNewEntityAsync(UserAddDto dto)
    {
        var user = new User { UserName = dto.UserName, PasswordSalt = HashCrypto.BuildSalt() };
        user.PasswordHash = HashCrypto.GeneratePwd(dto.Password, user.PasswordSalt);
        if (dto.Email != null)
        {
            user.Email = dto.Email;
        }
        await AddAsync(user);
        return user;
    }

    public override async Task<User> UpdateAsync(User entity, UserUpdateDto dto)
    {
        if (dto.Password != null && _userContext != null && _userContext.IsAdmin)
        {
            entity.PasswordSalt = HashCrypto.BuildSalt();
            entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);
        }
        return await base.UpdateAsync(entity, dto);
    }

    public override async Task<PageList<UserItemDto>> FilterAsync(UserFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.UserName, q => q.UserName == filter.UserName)
            .WhereNotNull(filter.UserType, q => q.UserType == filter.UserType)
            .WhereNotNull(filter.Email, q => q.Email == filter.Email)
            .WhereNotNull(filter.PhoneNumber, q => q.PhoneNumber == filter.PhoneNumber)
            .WhereNotNull(filter.EmailConfirmed, q => q.EmailConfirmed == filter.EmailConfirmed)
            .WhereNotNull(
                filter.PhoneNumberConfirmed,
                q => q.PhoneNumberConfirmed == filter.PhoneNumberConfirmed
            );

        return await Query.FilterAsync<UserItemDto>(
            Queryable,
            filter.PageIndex,
            filter.PageSize,
            filter.OrderBy
        );
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User?> GetOwnedAsync(Guid id)
    {
        IQueryable<User> query = Command.Db.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }
}
