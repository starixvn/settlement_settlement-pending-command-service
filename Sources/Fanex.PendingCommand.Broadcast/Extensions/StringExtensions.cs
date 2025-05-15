using Fanex.PendingCommand.Broadcast.Models;
using System.Text.Json;
using System.Xml.Serialization;

namespace Fanex.PendingCommand.Broadcast.Extensions;

public static class StringExtensions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static ResourcesModel ToResources(this string selectedResourced)
    {
        var serializer = new XmlSerializer(typeof(ResourcesModel));
        using var sr = new StringReader(selectedResourced);

        return (ResourcesModel)serializer.Deserialize(sr);
    }
}