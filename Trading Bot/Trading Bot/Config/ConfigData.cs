using System.Xml.Serialization;

namespace Trading_Bot.Config
{
    // Private class for serialization
    [XmlRoot("Configuration")]
    public class ConfigData
    {
        [XmlElement("ModelProjectPath")]
        public string ModelProjectPath { get; set; }
        [XmlElement("ModelLocalPath")]
        public string ModelLocalPath { get; set; }
        [XmlElement("TrainingDataPath")]
        public string TrainingDataPath { get; set; }
        [XmlElement("RiskTolerance")]
        public double RiskTolerance { get; set; }
        [XmlElement("Interval")]
        public int Interval { get; set; }
        [XmlElement("SequenceLength")]
        public int SequenceLength { get; set; }
    }
}
