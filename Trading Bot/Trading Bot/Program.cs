using Trading_Bot.Config;
using Trading_Bot.Model;

namespace Trading_Bot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Configuration.Load(".\\Config.xml");

            float[] sourceData = new float[]
            {
                0.41186284f ,
                0.41314922f ,
                0.41184274f ,
                0.41075736f ,
                0.40959157f ,
                0.41011417f ,
                0.41152114f ,
                0.41019457f ,
                0.4146366f ,
                0.41320952f
            };

            var prediction = ModelInvoker.Predict(sourceData);
            Console.WriteLine(prediction);
        }
    }
}
