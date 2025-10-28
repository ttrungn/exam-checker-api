using Exam.API.Models.Responses;
using Exam.Services.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Exam.API.Infrastructures;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(InvalidOperationException), HandleInvalidOperationException },
            { typeof(ArgumentException), HandleArgumentException },
            { typeof(ArgumentNullException), HandleArgumentException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedException), HandleUnauthorizedException },
            { typeof(AppException), HandleAppException }
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var handler = _exceptionHandlers
            .FirstOrDefault(h => h.Key.IsAssignableFrom(exceptionType)).Value;

        if (handler is null)
        {
            await HandleInternalServerError(httpContext, exception);
            return true;
        }

        await handler.Invoke(httpContext, exception);
        return true;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new DataApiResponse<IDictionary<string, string[]>>()
        {
            Success = false, Message = "Invalid request", Data = exception.Errors
        });
    }


    private async Task HandleInvalidOperationException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse() { Success = false, Message = ex.Message });
    }

    private async Task HandleArgumentException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse() { Success = false, Message = ex.Message });
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse { Success = false, Message = ex.Message });
    }

    private async Task HandleUnauthorizedException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse { Success = false, Message = ex.Message });
    }

    private async Task HandleAppException(HttpContext httpContext, Exception ex)
    {
        var appEx = (AppException)ex;
        httpContext.Response.StatusCode = appEx.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse { Success = false, Message = appEx.Message });
    }

    private async Task HandleInternalServerError(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new BaseApiResponse()
        {
            Success = false, Message = "Internal server error occurred."
        });
    }
}
