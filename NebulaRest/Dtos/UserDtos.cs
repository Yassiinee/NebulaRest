namespace NebulaRest.Dtos;

public record UserDto(int Id, string Name, string Email);

public record CreateUserDto(
    string Name,
    string Email
);

public record UpdateUserDto(
    string Name,
    string Email
);
