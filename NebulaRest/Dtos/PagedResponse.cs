namespace NebulaRest.Dtos;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}