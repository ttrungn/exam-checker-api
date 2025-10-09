using Microsoft.AspNetCore.Http;

namespace User.Services.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }

    public NotFoundException(string message, Exception? inner) : base(message, inner, StatusCodes.Status404NotFound)
    {
    }
}
