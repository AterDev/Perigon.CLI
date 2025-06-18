using IdentityServer.Definition.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IdentityServerContext db, LoginManager loginManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        => await db.Accounts.Include(a => a.AccountRoles).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account?>> GetById(Guid id)
        => await db.Accounts.Include(a => a.AccountRoles).FirstOrDefaultAsync(a => a.Id == id) is { } acc ? acc : NotFound();

    [HttpPost]
    public async Task<ActionResult<Account>> Create(Account account)
    {
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Account account)
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
    public async Task<IActionResult> Delete(Guid id)
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
    public async Task<IActionResult> SetRoles(Guid id, List<Guid> roleIds)
    {
        var acc = await db.Accounts.Include(a => a.AccountRoles).FirstOrDefaultAsync(a => a.Id == id);
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
    public async Task<ActionResult<Managers.LoginResult>> Login([FromBody] LoginRequest request)
    {
        var user = await db.Accounts
            .Include(a => a.AccountRoles)
                .ThenInclude(ar => ar.Role)
            .FirstOrDefaultAsync(a => a.UserName == request.UserName);

        var result = loginManager.ValidateLogin(user, request.Password);
        if (!result.IsSuccess)
        {
            return result;
        }

        // 登录成功，写 Cookie
        var claims = new List<Claim>
        {
            new("sub", user!.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, ConstVal.SuperAdmin)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return result;
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
