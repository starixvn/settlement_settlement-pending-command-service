using System.Xml.Serialization;

namespace Fanex.PendingCommand.Broadcast.Models;

public class ResourceItemModel
{
    [XmlAttribute("Id")]
    public string Id { get; set; }

    [XmlAttribute("Home")]
    public string Home { get; set; }

    [XmlAttribute("Away")]
    public string Away { get; set; }

    [XmlIgnore]
    public bool IsDuplicated { get; set; }
}