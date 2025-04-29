namespace Trading_Bot.Logger
{
    /// <summary>
    /// Logs prices into text file.
    /// </summary>
    public class PriceLogger
    {
        private readonly string logFilePath;

        /// <summary>
        /// Creates new instance of <see cref="PriceLogger"/>.
        /// </summary>
        /// <param name="logFilePath">Path of logfile to log to.</param>
        public PriceLogger(string logFilePath = "prices.log")
        {
            this.logFilePath = logFilePath;
            File.WriteAllText(logFilePath, string.Empty);
        }

        /// <summary>
        /// Log price with time and price.
        /// </summary>
        /// <param name="time">Time of price.</param>
        /// <param name="price">Price at given time.</param>
        public void LogPrice(DateTime timestamp, decimal price)
        {
            // Retrieve Eastern Standard Time zone information
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            // Convert original time to Eastern Time
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp, easternZone);
            File.AppendAllText(logFilePath, $"{price:F2},{easternTime:O}\n");
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
