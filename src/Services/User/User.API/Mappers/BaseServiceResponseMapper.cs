using User.API.Models.Responses;
using User.Services.Models.Responses;

namespace User.API.Mappers;

public static class BaseServiceResponseMapper
{
    public static BaseApiResponse ToBaseApiResponse(this BaseServiceResponse baseServiceResponse)
    {
        return new BaseApiResponse() { Success = baseServiceResponse.Success, Message = baseServiceResponse.Message };
    }
}
