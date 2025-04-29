using Trading_Bot.Config;

namespace Trading_Bot.Logger
{
    internal class PredictionLogger
    {
        private readonly string logFilePath;

        /// <summary>
        /// Creates new instance of <see cref="PriceLogger"/>.
        /// </summary>
        /// <param name="logFilePath">Path of logfile to log to.</param>
        public PredictionLogger(string logFilePath = "predictions.log")
        {
            this.logFilePath = logFilePath;
            File.WriteAllText(logFilePath, string.Empty);
        }

        /// <summary>
        /// Log prediction with time the prediciton should coorespond to.
        /// </summary>
        /// <param name="price">Predicted price.</param>
        public void LogPrice(decimal price)
        {
            var currentTime = DateTime.Now;
            var timestamp = currentTime.AddSeconds(Configuration.Interval * Configuration.PredictionInterval);
            File.AppendAllText(logFilePath, $"{price:F2},{timestamp:O}\n");
        }

        /// <summary>
        /// Wipe logfile.
        /// </summary>
        public void ClearLog()
        {
            File.WriteAllText(logFilePath, string.Empty);
        }
    }
}
