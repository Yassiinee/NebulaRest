namespace NebulaRest.Dtos
{
    public record PostDto(
    int Id,
    string Title,
    string Content,
    int UserId,
    string UserName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

    public record CreatePostDto(
        string Title,
        string Content,
        int UserId);

    public record UpdatePostDto(
        string Title,
        string Content);
}
