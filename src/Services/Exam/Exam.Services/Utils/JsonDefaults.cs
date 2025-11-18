using System.Text.Json;

namespace Exam.Services.Utils;

public static  class JsonDefaults
{
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
}
