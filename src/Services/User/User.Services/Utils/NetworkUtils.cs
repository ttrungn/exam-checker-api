using Microsoft.AspNetCore.Http;

namespace PRN232.Lab1.CoffeeStore.Services.Utils;

public static class NetworkUtils
{
    public static string GetIpAddress(HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress;

        if (remoteIpAddress != null)
        {
            if (remoteIpAddress.IsIPv4MappedToIPv6)
            {
                return remoteIpAddress.MapToIPv4().ToString();
            }

            return remoteIpAddress.ToString();
        }

        throw new InvalidOperationException("Không tìm thấy địa chỉ IP");
    }
}