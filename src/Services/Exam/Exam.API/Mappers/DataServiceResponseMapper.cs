using Exam.API.Models.Responses;
using Exam.Services.Models.Responses;

namespace Exam.API.Mappers;

public static class DataServiceResponseMapper
{
    public static DataApiResponse<T> ToDataApiResponse<T>(this DataServiceResponse<T> dataServiceResponse,
        HttpRequest? request = null, HttpResponse? response = null)
    {
        return new DataApiResponse<T>()
        {
            Success = dataServiceResponse.Success,
            Message = dataServiceResponse.Message,
            Data = dataServiceResponse.Data
        };
    }
}
