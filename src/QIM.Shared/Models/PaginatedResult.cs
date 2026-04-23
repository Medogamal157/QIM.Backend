namespace QIM.Shared.Models;

/// <summary>
/// Paginated result wrapper for list endpoints.
/// </summary>
public class PaginatedResult<T> : Result<List<T>>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public static PaginatedResult<T> Success(List<T> data, int totalCount, int page, int pageSize) =>
        new()
        {
            IsSuccess = true,
            Data = data,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };

    public new static PaginatedResult<T> Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };
}
