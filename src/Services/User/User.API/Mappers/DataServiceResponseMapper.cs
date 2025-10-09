using User.API.Models.Responses;
using User.Services.Models.Responses;

namespace User.API.Mappers;

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
