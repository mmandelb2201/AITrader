using System.Xml.Serialization;

namespace Trading_Bot.Config
{
    /// <summary>
    /// Loads and stores the configuration for operating TradingBot.
    /// </summary>
    internal static class Configuration
    {
        // Static properties
        public static string ModelProjectPath { get; set; }
        public static string ModelLocalPath { get; set; }
        public static string TrainingDataPath { get; set; }
        public static double RiskTolerance { get; set; }
        public static int Interval { get; set; }
        public static int SequenceLength { get; set; }

        /// <summary>
        /// Loads the configuration file from the defined path.
        /// Prints out the found configuration.
        /// </summary>
        /// <param name="path">The path which the config xml is located.</param>
        public static void Load(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
            using StreamReader reader = new StreamReader(path);
            ConfigData data = (ConfigData)serializer.Deserialize(reader);
            ModelProjectPath = data.ModelProjectPath;
            ModelLocalPath = data.ModelLocalPath;
            TrainingDataPath = data.TrainingDataPath;
            RiskTolerance = data.RiskTolerance;
            Interval = data.Interval;
            SequenceLength = data.SequenceLength;
            PrintConfiguration();
        }

        private static void PrintConfiguration()
        {
            Console.WriteLine("Trading Bot started with following configuration:");
            Console.WriteLine("Model Project Path: {0}", ModelProjectPath);
            Console.WriteLine("Model Local Path: {0}", ModelLocalPath);
            Console.WriteLine("Training Data Path: {0}", TrainingDataPath);
            Console.WriteLine("Risk tolerance: " + RiskTolerance);
            Console.WriteLine("Interval: " + Interval);
            Console.WriteLine("Sequence length: " + SequenceLength);
        }
    }


}
