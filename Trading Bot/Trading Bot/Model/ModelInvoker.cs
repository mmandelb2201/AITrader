using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Trading_Bot.Config;
using Trading_Bot.Model.Exceptions;

namespace Trading_Bot.Model
{
    /// <summary>
    /// Interfaces with ONNX LSTM model to get a prediction.
    /// </summary>
    public static class ModelInvoker
    {
        /// <summary>
        /// Get prediction from an LSTM model.
        /// </summary>
        /// <param name="inputs">Array of length 10 of inputs.</param>
        /// <returns>Float prediction based on inputs.</returns>
        /// <exception cref="ArgumentException">Throws if inputs is not exactly 10 elements.</exception>
        /// <exception cref="ModelInvokeError">Throws if invoking model does not produce a prediction.</exception>
        public static float Predict(float[] inputs)
        {
            if (inputs is null || inputs.Length != 10)
                throw new ArgumentException("Predict must take an array of exactly 10 floats");
            
            var modelPath = Path.Combine(Configuration.ModelProjectPath, Configuration.ModelLocalPath);
            using var session = new InferenceSession(modelPath);
            var dimensions = new[] { 1, 10, 1 };

            // Convert the 1D array to a tensor with shape [10, 1]
            var tensor = new DenseTensor<float>(inputs, dimensions);
            var input = NamedOnnxValue.CreateFromTensor("input", tensor);

            var container = new List<NamedOnnxValue>() { input };
            using var runOptions = new RunOptions();

            using var results = session.Run(container);
            foreach (var result in results)
            {
                return result.AsTensor<float>().GetValue(0);
            }

            throw new ModelInvokeError("Invoking model gave 0 predictions.");
        }
    }
}
