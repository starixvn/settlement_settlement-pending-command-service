using System.Xml.Serialization;

namespace Fanex.PendingCommand.Broadcast.Models;

[XmlRoot("Resources")]
public class ResourcesModel
{
    [XmlElement("Resource")]
    public List<ResourceItemModel> Items { get; set; }
}