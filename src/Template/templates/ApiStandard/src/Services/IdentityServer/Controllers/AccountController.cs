using System.Security.Claims;
using IdentityServer.Definition.Entity;
using IdentityServer.Definition.Share.AccountDtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(
    IdentityServerContext db,
    LoginManager loginManager,
    Localizer localizer
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAllAsync() =>
        await db.Accounts.Include(a => a.AccountRoles).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account?>> GetByIdAsync(Guid id) =>
        await db.Accounts.Include(a => a.AccountRoles).FirstOrDefaultAsync(a => a.Id == id)
            is { } acc
            ? acc
            : NotFound();

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAsync(Account account)
    {
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByIdAsync), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, Account account)
    {
        if (id != account.Id)
        {
            return BadRequest();
        }

        db.Entry(account).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var acc = await db.Accounts.FindAsync(id);
        if (acc == null)
        {
            return NotFound();
        }

        db.Accounts.Remove(acc);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> SetRolesAsync(Guid id, List<Guid> roleIds)
    {
        var acc = await db
            .Accounts.Include(a => a.AccountRoles)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (acc == null)
        {
            return NotFound();
        }

        acc.AccountRoles.Clear();
        foreach (var rid in roleIds)
        {
            acc.AccountRoles.Add(new AccountRole { AccountId = id, RoleId = rid });
        }
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromForm] LoginRequest request)
    {
        var user = await db
            .Accounts.Include(a => a.AccountRoles)
            .ThenInclude(ar => ar.Role)
            .FirstOrDefaultAsync(a => a.UserName == request.UserName);

        var result = loginManager.ValidateLogin(user, request.Password);
        if (!result.IsSuccess)
        {
            // 登录失败，重定向回登录页并附带错误信息
            return Redirect(
                $"/login?error={Uri.EscapeDataString(result.ErrorMessage ?? localizer.Get(LanguageKey.Login) + localizer.Get(LanguageKey.Failed))}"
            );
        }

        // 登录成功，写 Cookie
        var claims = new List<Claim>
        {
            new("sub", user!.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, ConstVal.SuperAdmin),
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Redirect("/");
    }

    /// <summary>
    /// logout
    /// </summary>
    /// <returns></returns>
    [HttpGet("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
