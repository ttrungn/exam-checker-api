using System.Text;

namespace Exam.Services.Utils;

public static class StringUtils
{
    public static string BuildOrEqualsFilter(string field, IEnumerable<string> values)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var v in values)
        {
            var safe = v.Replace("'", "''");
            if (!first)
            {
                sb.Append(" or ");
            }

            sb.Append($"{field} eq '{safe}'");
            first = false;
        }

        return sb.ToString();
    }
}
