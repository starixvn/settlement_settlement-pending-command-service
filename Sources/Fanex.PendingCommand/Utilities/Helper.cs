using System.Text.Json;

namespace Fanex.PendingCommand.Utilities;

public static class Helper
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}