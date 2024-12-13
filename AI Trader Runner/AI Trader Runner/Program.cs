using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Trader_Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Configuration.Load("C:\\Users\\mattr\\OneDrive\\Desktop\\src\\AITrader\\AI Trader Runner\\AI Trader Runner\\Config.xml");
            Console.WriteLine(Configuration.RiskTolerance);
            new PricePredictor().Predict();
        }
    }
}
