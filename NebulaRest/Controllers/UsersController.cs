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
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;
    private const int CacheDurationSeconds = 60;

    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpGet]
    [OutputCache(Duration = CacheDurationSeconds, VaryByQueryKeys = new[] { "page", "pageSize" })]
    public async Task<ActionResult<IEnumerable<UserDto>>> Get(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        var validatedPage = ValidatePage(page);
        var validatedPageSize = ValidatePageSize(pageSize);

        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip((validatedPage - 1) * validatedPageSize)
            .Take(validatedPageSize)
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound();

        var etag = GenerateETag(user.RowVersion);

        if (IsETagMatch(etag))
            return NotModified(etag);

        SetETagHeader(etag);
        return Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        if (!IsValidCreateDto(dto))
            return BadRequest("Name and Email are required.");

        var entity = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim()
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        var result = new UserDto(entity.Id, entity.Name, entity.Email);
        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.Id, version = "1.0" },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!IsValidUpdateDto(dto))
            return BadRequest("Name and Email are required.");

        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (entity is null)
            return NotFound();

        entity.Name = dto.Name.Trim();
        entity.Email = dto.Email.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Users.FindAsync(id);

        if (entity is null)
            return NotFound();

        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Private helper methods
    private static int ValidatePage(int page) =>
        page < 1 ? DefaultPage : page;

    private static int ValidatePageSize(int pageSize) =>
        pageSize < 1 || pageSize > MaxPageSize ? DefaultPageSize : pageSize;

    private static string GenerateETag(byte[]? rowVersion) =>
        rowVersion is { Length: > 0 }
            ? $"W/\"{Convert.ToBase64String(rowVersion)}\""
            : "\"0\"";

    private bool IsETagMatch(string etag) =>
        Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) &&
        ifNoneMatch.Any(v => v == etag);

    private ActionResult NotModified(string etag)
    {
        SetETagHeader(etag);
        return StatusCode(304);
    }

    private void SetETagHeader(string etag) =>
        Response.Headers["ETag"] = etag;

    private static bool IsValidCreateDto(CreateUserDto? dto) =>
        dto is not null &&
        !string.IsNullOrWhiteSpace(dto.Name) &&
        !string.IsNullOrWhiteSpace(dto.Email);

    private static bool IsValidUpdateDto(UpdateUserDto? dto) =>
        dto is not null &&
        !string.IsNullOrWhiteSpace(dto.Name) &&
        !string.IsNullOrWhiteSpace(dto.Email);
}
