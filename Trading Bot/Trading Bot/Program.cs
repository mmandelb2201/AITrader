using Trading_Bot.Config;
using Trading_Bot.Model;
using Trading_Bot.Trader;

namespace Trading_Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Configuration.Load("Config.xml");
            Initialize();
            var (isBuy, portfolioFraction) = await TradingSequence.TradingStepAsync().ConfigureAwait(false);
        }

        private static void Initialize()
        {
            var trainingDataPath = Path.Combine(Configuration.ModelProjectPath, Configuration.TrainingDataPath);
            MinMaxScaler.Fit(trainingDataPath);
        }
    }
}
