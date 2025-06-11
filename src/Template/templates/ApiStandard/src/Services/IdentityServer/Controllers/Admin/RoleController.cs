using IdentityServer.Definition.Entity;
using IdentityServer.Definition.EntityFramework;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers.Admin;

[ApiController]
[Route("api/admin/role")]
public class RoleController : ControllerBase
{
    private readonly IdentityServerContext _db;
    public RoleController(IdentityServerContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        => await _db.Roles.Include(r => r.RolePermissions).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Role?>> GetById(Guid id)
        => await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == id) is { } role ? role : NotFound();

    [HttpPost]
    public async Task<ActionResult<Role>> Create(Role role)
    {
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Role role)
    {
        if (id != role.Id)
        {
            return BadRequest();
        }

        _db.Entry(role).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> SetPermissions(Guid id, List<Guid> permissionIds)
    {
        var role = await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            return NotFound();
        }

        role.RolePermissions.Clear();
        foreach (var pid in permissionIds)
        {
            role.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = pid });
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
