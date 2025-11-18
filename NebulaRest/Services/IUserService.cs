using NebulaRest.Dtos;

namespace NebulaRest.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsersAsync(int page, int pageSize);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(int id);
}