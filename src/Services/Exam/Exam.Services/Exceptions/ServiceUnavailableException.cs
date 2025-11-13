using Microsoft.AspNetCore.Http;

namespace Exam.Services.Exceptions;

public sealed class ServiceUnavailableException : AppException
{
    public ServiceUnavailableException(string message) : base(message, StatusCodes.Status500InternalServerError)
    {
    }

    public ServiceUnavailableException(string message, Exception? inner) : base(message, inner,
        StatusCodes.Status404NotFound)
    {
    }
}
