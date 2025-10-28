namespace Exam.Services.Models.Responses;

public class DataServiceResponse<T> : BaseServiceResponse
{
    public T Data { get; set; } = default!;
}
