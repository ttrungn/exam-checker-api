namespace Exam.API.Models.Responses;

public class PaginationApiResponse<T> : BaseApiResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalCurrentCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex  < TotalPages;
    public IList<T> Data { get; set; } = new List<T>();
}
