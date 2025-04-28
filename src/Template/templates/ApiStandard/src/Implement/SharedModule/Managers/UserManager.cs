using Entity.UserMod;
using Share.Models.UserDtos;

namespace SharedModule.Managers;
/// <summary>
/// 用户账户
/// </summary>
public class UserManager(
    DataAccessContext<User> dataContext,
    ILogger<UserManager> logger,
    UserContext userContext) : ManagerBase<User>(dataContext, logger)
{
    private readonly UserContext _userContext = userContext;

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
        return await SaveChangesAsync() > 0;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        var user = new User
        {
            UserName = dto.UserName,
            PasswordSalt = HashCrypto.BuildSalt()
        };
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
    public async Task<Guid?> AddAsync(UserAddDto dto)
    {
        var entity = new User
        {
            UserName = dto.UserName,
            PasswordSalt = HashCrypto.BuildSalt()
        };
        entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);
        if (dto.Email != null)
        {
            entity.Email = dto.Email;
        }
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(User entity, UserUpdateDto dto)
    {
        entity.Merge(dto);
        if (dto.Password != null && _userContext != null && _userContext.IsAdmin)
        {
            entity.PasswordSalt = HashCrypto.BuildSalt();
            entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);
        }
        return await UpdateAsync(entity);
    }

    public async Task<PageList<UserItemDto>> ToPageAsync(UserFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.UserName, q => q.UserName == filter.UserName)
            .WhereNotNull(filter.UserType, q => q.UserType == filter.UserType)
            .WhereNotNull(filter.Email, q => q.Email == filter.Email)
            .WhereNotNull(filter.PhoneNumber, q => q.PhoneNumber == filter.PhoneNumber)
            .WhereNotNull(filter.EmailConfirmed, q => q.EmailConfirmed == filter.EmailConfirmed)
            .WhereNotNull(filter.PhoneNumberConfirmed, q => q.PhoneNumberConfirmed == filter.PhoneNumberConfirmed);

        return await ToPageAsync<UserFilterDto, UserItemDto>(filter);
    }

    /// <summary>
    /// 唯一性判断
    /// </summary>
    /// <param name="unique">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string unique, Guid? id = null)
    {
        // TODO:自定义唯一性验证参数和逻辑
        return await Command.Where(q => q.UserName == unique)
            .WhereNotNull(id, q => q.Id != id)
            .AnyAsync();
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<UserDetailDto?> GetDetailAsync(Guid id)
    {
        return await FindAsync<UserDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// 数据权限验证
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<User?> GetOwnedAsync(Guid id)
    {
        var query = Command.Where(q => q.Id == id);
        // TODO:自定义数据权限验证
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
