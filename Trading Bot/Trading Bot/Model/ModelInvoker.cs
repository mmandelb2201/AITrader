using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Trading_Bot.Config;
using Trading_Bot.Model.Exceptions;

namespace Trading_Bot.Model
{
    public static class ModelInvoker
    {
        public static float Predict(float[] inputs)
        {
            if (inputs is null || inputs.Length != 10)
                throw new ArgumentException("Predict must take an array of exactly 10 floats");
            
            using var session = new InferenceSession(Configuration.ModelFilePath);
            var dimensions = new[] { 1, 10, 1 };

            // Convert the 1D array to a tensor with shape [10, 1]
            var tensor = new DenseTensor<float>(inputs, dimensions);
            var input = NamedOnnxValue.CreateFromTensor("input_1", tensor);

            var container = new List<NamedOnnxValue>() { input };
            using var runOptions = new RunOptions();

            using (var results = session.Run(container))
            {
                foreach (var result in results)
                {
                    return result.AsTensor<float>().GetValue(0);
                }
            }

            throw new ModelInvokeError("Error Invoking Model");
        }
    }
}
