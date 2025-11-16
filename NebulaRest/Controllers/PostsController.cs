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
public class PostsController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;
    private const int CacheDurationSeconds = 60;

    private readonly AppDbContext _db;
    private readonly ILogger<PostsController> _logger;

    public PostsController(AppDbContext db, ILogger<PostsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get all posts with optional filtering by userId
    /// </summary>
    [HttpGet]
    [OutputCache(Duration = CacheDurationSeconds, VaryByQueryKeys = new[] { "page", "pageSize", "userId" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PostDto>>> Get(
        [FromQuery] int page = DefaultPage,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] int? userId = null)
    {
        var validatedPage = ValidatePage(page);
        var validatedPageSize = ValidatePageSize(pageSize);

        var query = _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);

        var posts = await query
            .Skip((validatedPage - 1) * validatedPageSize)
            .Take(validatedPageSize)
            .Select(p => new PostDto(
                p.Id,
                p.Title,
                p.Content,
                p.UserId,
                p.User.Name,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync();

        return Ok(posts);
    }

    /// <summary>
    /// Get a specific post by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDto>> GetById(int id)
    {
        var post = await _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            _logger.LogInformation("Post {PostId} not found", id);
            return NotFound();
        }

        var etag = GenerateETag(post.RowVersion);

        if (IsETagMatch(etag))
        {
            _logger.LogDebug("ETag match for post {PostId}, returning 304", id);
            return NotModified(etag);
        }

        SetETagHeader(etag);
        return Ok(new PostDto(
            post.Id,
            post.Title,
            post.Content,
            post.UserId,
            post.User.Name,
            post.CreatedAt,
            post.UpdatedAt));
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostDto dto)
    {
        if (!IsValidCreateDto(dto))
        {
            _logger.LogWarning("Invalid create post DTO received");
            return BadRequest("Title, Content, and UserId are required.");
        }

        // Verify user exists
        var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
        {
            _logger.LogWarning("Attempted to create post with non-existent user {UserId}", dto.UserId);
            return BadRequest($"User with ID {dto.UserId} does not exist.");
        }

        var now = DateTime.UtcNow;
        var entity = new Post
        {
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            UserId = dto.UserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Posts.Add(entity);
        await _db.SaveChangesAsync();

        // Load user for response
        await _db.Entry(entity).Reference(p => p.User).LoadAsync();

        _logger.LogInformation("Created post {PostId} for user {UserId}", entity.Id, dto.UserId);

        var result = new PostDto(
            entity.Id,
            entity.Title,
            entity.Content,
            entity.UserId,
            entity.User.Name,
            entity.CreatedAt,
            entity.UpdatedAt);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.Id, version = "1.0" },
            result);
    }

    /// <summary>
    /// Update an existing post
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePostDto dto)
    {
        if (!IsValidUpdateDto(dto))
        {
            _logger.LogWarning("Invalid update post DTO received for post {PostId}", id);
            return BadRequest("Title and Content are required.");
        }

        var entity = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);

        if (entity is null)
        {
            _logger.LogInformation("Post {PostId} not found for update", id);
            return NotFound();
        }

        entity.Title = dto.Title.Trim();
        entity.Content = dto.Content.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated post {PostId}", id);
        return NoContent();
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Posts.FindAsync(id);

        if (entity is null)
        {
            _logger.LogInformation("Post {PostId} not found for deletion", id);
            return NotFound();
        }

        _db.Posts.Remove(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted post {PostId}", id);
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

    private static bool IsValidCreateDto(CreatePostDto? dto) =>
        dto is not null &&
        !string.IsNullOrWhiteSpace(dto.Title) &&
        !string.IsNullOrWhiteSpace(dto.Content) &&
        dto.UserId > 0;

    private static bool IsValidUpdateDto(UpdatePostDto? dto) =>
        dto is not null &&
        !string.IsNullOrWhiteSpace(dto.Title) &&
        !string.IsNullOrWhiteSpace(dto.Content);
}