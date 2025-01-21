using Trading_Bot.Config;
using Trading_Bot.Model;

namespace Trading_Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Configuration.Load("Config.xml");
            Initialize();
            await Predictor.PredictAsync().ConfigureAwait(false);
        }

        private static void Initialize()
        {
            var trainingDataPath = Path.Combine(Configuration.ModelProjectPath, Configuration.TrainingDataPath);
            MinMaxScaler.Fit(trainingDataPath);
        }
    }
}
