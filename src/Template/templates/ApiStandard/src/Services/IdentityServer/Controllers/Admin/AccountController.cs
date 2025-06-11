using IdentityServer.Definition.Entity;
using IdentityServer.Definition.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Controllers.Admin;

[ApiController]
[Route("api/admin/account")]
public class AccountController : ControllerBase
{
    private readonly IdentityServerContext _db;
    public AccountController(IdentityServerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        => await _db.Accounts.Include(a => a.AccountRoles).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account?>> GetById(Guid id)
        => await _db.Accounts.Include(a => a.AccountRoles).FirstOrDefaultAsync(a => a.Id == id) is { } acc ? acc : NotFound();

    [HttpPost]
    public async Task<ActionResult<Account>> Create(Account account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Account account)
    {
        if (id != account.Id)
        {
            return BadRequest();
        }

        _db.Entry(account).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var acc = await _db.Accounts.FindAsync(id);
        if (acc == null)
        {
            return NotFound();
        }

        _db.Accounts.Remove(acc);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, List<Guid> roleIds)
    {
        var acc = await _db.Accounts.Include(a => a.AccountRoles).FirstOrDefaultAsync(a => a.Id == id);
        if (acc == null)
        {
            return NotFound();
        }

        acc.AccountRoles.Clear();
        foreach (var rid in roleIds)
        {
            acc.AccountRoles.Add(new AccountRole { AccountId = id, RoleId = rid });
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
