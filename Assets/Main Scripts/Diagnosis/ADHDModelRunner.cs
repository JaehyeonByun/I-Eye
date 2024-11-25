using Unity.Barracuda;
using UnityEngine;

public class ADHDModelRunner : MonoBehaviour
{
    public NNModel modelAsset; 
    private IWorker worker;
    private Model model;
    private int batch = 1;
    private int channel = 1;
    private int height = 6;
    private int width = 3;

    public static float[] resultArray = new float[4] {0f, 0f, 0f, 0f};

    void Start()
    {
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    public float[] ReorderInput(float[] inputArray)
    {
        float[] reorderedInput = new float[inputArray.Length];
        int width = 3;
        int height = 6;
        int channels = 1;

        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                for (int c = 0; c < channels; c++)
                {
                    reorderedInput[h * width * channels + w * channels + c] =
                        inputArray[c * height * width + h * width + w];
                }
            }
        }

        return reorderedInput;
    }

    public float[] RunModel(float[] inputArray)
    {
        Tensor inputTensor = new Tensor(batch, height, width, channel, ReorderInput(inputArray));
        
        worker.Execute(inputTensor);

        Tensor outputTensor = worker.PeekOutput();
        float[] outputArray = outputTensor.ToReadOnlyArray();
        
        inputTensor.Dispose();
        outputTensor.Dispose();

        resultArray = outputArray;

        return outputArray;
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}
