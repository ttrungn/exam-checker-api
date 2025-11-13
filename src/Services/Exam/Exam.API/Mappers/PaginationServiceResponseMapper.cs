using Exam.API.Models.Responses;
using Exam.Services.Models.Responses;

namespace Exam.API.Mappers;

public static class PaginationServiceResponseMapper
{
    public static PaginationApiResponse<T> ToPaginationApiResponse<T>(this PaginationServiceResponse<T> paginationServiceResponse)
    {
        return new PaginationApiResponse<T>()
        {
            Success = paginationServiceResponse.Success,
            Message = paginationServiceResponse.Message,
            PageIndex = paginationServiceResponse.PageIndex,
            PageSize = paginationServiceResponse.PageSize,
            TotalCount = paginationServiceResponse.TotalCount,
            TotalCurrentCount = paginationServiceResponse.TotalCurrentCount,
            TotalPages = paginationServiceResponse.TotalPages,
            Data = paginationServiceResponse.Data
        };
    }
}
