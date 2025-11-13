namespace Exam.Services.Models.Responses;

public class PaginationServiceResponse<T> : BaseServiceResponse
{
    /// <summary>
    ///     Gets or sets the index of the page.
    /// </summary>
    /// <value>The index of the page.</value>
    public int PageIndex { get; set; }

    /// <summary>
    ///     Gets or sets the size of the page.
    /// </summary>
    /// <value>The size of the page.</value>
    public int PageSize { get; set; }

    /// <summary>
    ///     Gets or sets the total count.
    /// </summary>
    /// <value>The total count.</value>
    public int TotalCount { get; set; }

    /// <summary>
    ///     Gets or sets the total current count.
    /// </summary>
    /// <value>The current total count.</value>
    public int TotalCurrentCount { get; set; }

    /// <summary>
    ///     Gets or sets the total pages.
    /// </summary>
    /// <value>The total pages.</value>
    public int TotalPages { get; set; }

    /// <summary>
    ///     Gets or sets the items.
    /// </summary>
    /// <value>The data.</value>
    public IList<T> Data { get; set; } = new List<T>();

    /// <summary>
    ///     Gets the has previous page.
    /// </summary>
    /// <value>The has previous page.</value>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>
    ///     Gets the has next page.
    /// </summary>
    /// <value>The has next page.</value>
    public bool HasNextPage => PageIndex < TotalPages;
}
