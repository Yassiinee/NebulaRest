using Microsoft.EntityFrameworkCore;
using NebulaRest.Data;
using NebulaRest.Dtos;
using NebulaRest.Entities;

namespace NebulaRest.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(int page, int pageSize)
    {
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Name, u.Email))
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : new UserDto(user.Id, user.Name, user.Email);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var entity = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim()
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created user {UserId}", entity.Id);

        return new UserDto(entity.Id, entity.Name, entity.Email);
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var entity = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (entity is null) return false;

        entity.Name = dto.Name.Trim();
        entity.Email = dto.Email.Trim();

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", id);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity is null) return false;

        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted user {UserId}", id);
        return true;
    }

    public async Task<bool> UserExistsAsync(int id)
    {
        return await _db.Users.AnyAsync(u => u.Id == id);
    }
}