namespace Exam.API.Models.Responses;

public class DataApiResponse<T> : BaseApiResponse
{
    public T Data { get; set; } = default!;
}
