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
            long[] dimensions =
            {
                10, 1
            };

            using var inputOrtValue = OrtValue.CreateTensorValueFromMemory(sourceData, dimensions);

            // Convert the 1D array to a tensor with shape [10, 1]
            var tensor = new DenseTensor<float>(sourceData, new[] { 10, 1 });

            // Create a sequence containing the tensor
            var sequence = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_1", tensor)
            };

            var namedOnnxValue = NamedOnnxValue.CreateFromSequence("input_1", sequence);

            var inputs = new Dictionary<string, OrtValue> {
                { "input_1",  inputOrtValue }
            };

            using var runOptions = new RunOptions();

            // Pass inputs and request the first output
            // Note that the output is a disposable collection that holds OrtValues
            using var output = session.Run(new List<NamedOnnxValue> { namedOnnxValue } , session.OutputNames, runOptions);

            var output_0 = output[0];

            // Assuming the output contains a tensor of float data, you can access it as follows
            // Returns Span<float> which points directly to native memory.
            var outputData = output_0.Value;
            Console.WriteLine($"Output is : {outputData}");
        }
    }
}
