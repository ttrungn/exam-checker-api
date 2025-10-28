namespace Exam.Services.Exceptions;

public class AppException : Exception
{
    protected AppException(string message, int statusCode, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    protected AppException(string message, Exception? inner, int statusCode, string? errorCode = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
