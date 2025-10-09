namespace User.Services.Models.Responses;

public class DataServiceResponse<T> : BaseServiceResponse
{
    public T Data { get; set; } = default!;
}
