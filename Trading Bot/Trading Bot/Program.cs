using Trading_Bot.Coinbase;
using Trading_Bot.Config;
using Trading_Bot.Model;
using Trading_Bot.Trader;

namespace Trading_Bot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            /*var helper = new WalletHelper("./.env");
            var usdAccount = await helper.GetUsdAccountAsync().ConfigureAwait(false);
            Console.WriteLine("Amount: " + usdAccount.AvailableBalance.DecimalValue);
            var product = "ETH-USD";
            var coinbaseClient = new CoinbaseClient();
            var ethProduct = await coinbaseClient.GetProductAsync(product).ConfigureAwait(false);
            Console.WriteLine($"{product} price: {ethProduct.Price}");*/

            Configuration.Load("Config.xml");
            Initialize();
            await RunSequenceAsync().ConfigureAwait(false);
        }

        private static async Task RunSequenceAsync()
        {
            Console.WriteLine("Enter 'q' to quit the program");

            while (true)
            {
                Console.WriteLine($"INFO: Executing run sequence at: {DateTime.Now.ToShortTimeString()}");
                var (isBuy, portfolioFraction) = await TradingSequence.PredictionStepAsync().ConfigureAwait(false);
                Console.WriteLine($"INFO: isBuy:{isBuy} portfolioFraction: {portfolioFraction}");

                // Check if the user has pressed a key
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                    {
                        Console.WriteLine("Exiting program.");
                        break;
                    }
                }

                await Task.Delay(1000 * Configuration.Interval * Configuration.SequenceLength).ConfigureAwait(false);
            };
        }

        private static void Initialize()
        {
            var trainingDataPath = Path.Combine(Configuration.ModelProjectPath, Configuration.TrainingDataPath);
            MinMaxScaler.Fit(trainingDataPath);
        }
    }
}
