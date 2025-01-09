using System.Xml.Serialization;

namespace Trading_Bot.Config
{
    // Private class for serialization
    [XmlRoot("Configuration")]
    public class ConfigData
    {
        [XmlElement("ModelFilePath")]
        public string ModelFilePath { get; set; }

        [XmlElement("RiskTolerance")]
        public double RiskTolerance { get; set; }
    }
}
