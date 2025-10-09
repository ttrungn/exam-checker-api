using Microsoft.AspNetCore.Http;

namespace User.Services.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message, StatusCodes.Status401Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception? inner) : base(message, inner,
        StatusCodes.Status401Unauthorized)
    {
    }
}
