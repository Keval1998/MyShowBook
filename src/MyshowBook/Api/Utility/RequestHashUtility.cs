using System.Security.Cryptography;
using System.Text;

namespace MyShowBook.Api.Utility;

public static class RequestHashUtility
{
    public static string Create(Guid showGuid, IEnumerable<string> seats)
    {
        var normalizedSeats = seats
            .Select(NormalizeSeat)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var payload = showGuid.ToString("D") + "|" + string.Join(",", normalizedSeats);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string NormalizeSeat(string seat) =>
        seat.Trim().ToUpperInvariant();
}
