using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Trader_Runner
{
    internal class PricePredictor
    {
        public void Predict()
        {
            using var session = new InferenceSession(Configuration.ModelFilePath);

            float[] sourceData  = new float[]
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
            var dimensions = new[] { 1, 10, 1 };


            // Convert the 1D array to a tensor with shape [10, 1]
            var tensor = new DenseTensor<float>(sourceData, dimensions);
            var input = NamedOnnxValue.CreateFromTensor("input_1", tensor);

            var container = new List<NamedOnnxValue>() { input };
            using var runOptions = new RunOptions();

            using(var results = session.Run(container))
            {
                foreach (var result in results)
                {
                    Console.WriteLine($"Result: {result.AsTensor<float>().GetArrayString()}");
                }
            }
        }
    }
}
