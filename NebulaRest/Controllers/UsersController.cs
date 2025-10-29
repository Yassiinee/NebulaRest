using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using NebulaRest.Data;
using NebulaRest.Dtos;
using NebulaRest.Entities;

namespace NebulaRest.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "page", "pageSize" })]
    public async Task<ActionResult<IEnumerable<UserDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        var query = _db.Users.AsNoTracking().OrderBy(u => u.Id);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var etag = MakeEtag(user.RowVersion);
        if (Request.Headers.TryGetValue("If-None-Match", out var ifNone) && ifNone.Any(v => v == etag))
        {
            Response.Headers["ETag"] = etag;
            return StatusCode(304);
        }

        Response.Headers["ETag"] = etag;
        return Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest();

        var entity = new User { Name = dto.Name.Trim(), Email = dto.Email.Trim() };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        var result = new UserDto(entity.Id, entity.Name, entity.Email);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null) return NotFound();
        if (dto is null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest();

        entity.Name = dto.Name.Trim();
        entity.Email = dto.Email.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity is null) return NotFound();

        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string MakeEtag(byte[]? rowVersion)
        => rowVersion is { Length: > 0 } ? $"W/\"{Convert.ToBase64String(rowVersion)}\"" : "\"0\"";
}
