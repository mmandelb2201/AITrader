using System.Xml.Serialization;

namespace Trading_Bot.Config
{
    internal static class Configuration
    {
        // Static properties
        public static string ModelFilePath { get; set; }
        public static double RiskTolerance { get; set; }

        // Method to load configuration from XML
        public static void Load(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
            using StreamReader reader = new StreamReader(path);
            ConfigData data = (ConfigData)serializer.Deserialize(reader);
            ModelFilePath = data.ModelFilePath;
            RiskTolerance = data.RiskTolerance;
        }
    }


}
