using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AI_Trader_Runner
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
