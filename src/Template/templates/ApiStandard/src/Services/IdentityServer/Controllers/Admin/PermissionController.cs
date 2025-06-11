using IdentityServer.Definition.Entity;
using IdentityServer.Definition.EntityFramework;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.Admin;

[ApiController]
[Route("api/admin/permission")]
public class PermissionController : ControllerBase
{
    private readonly IdentityServerContext _db;
    public PermissionController(IdentityServerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Permission>>> GetAll()
        => await _db.Permissions.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Permission?>> GetById(Guid id)
        => await _db.Permissions.FindAsync(id) is { } perm ? perm : NotFound();

    [HttpPost]
    public async Task<ActionResult<Permission>> Create(Permission permission)
    {
        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Permission permission)
    {
        if (id != permission.Id)
        {
            return BadRequest();
        }

        _db.Entry(permission).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var perm = await _db.Permissions.FindAsync(id);
        if (perm == null)
        {
            return NotFound();
        }

        _db.Permissions.Remove(perm);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
