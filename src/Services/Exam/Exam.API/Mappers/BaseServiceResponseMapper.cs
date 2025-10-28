using Exam.API.Models.Responses;
using Exam.Services.Models.Responses;

namespace Exam.API.Mappers;

public static class BaseServiceResponseMapper
{
    public static BaseApiResponse ToBaseApiResponse(this BaseServiceResponse baseServiceResponse)
    {
        return new BaseApiResponse() { Success = baseServiceResponse.Success, Message = baseServiceResponse.Message };
    }
}
