using Unity.Barracuda;
using UnityEngine;

public class ADHDModelRunner : MonoBehaviour
{
    // Dummy input
    // public float[] dummyArray = {0.2639f, 0.3876f, 0.0653f, 0.9278f, 0.8771f, 0.4289f, 0.9481f, 0.4997f, 0.3194f, 0.6469f, 0.3467f, 0.5099f, 0.2347f, 0.2203f, 0.0713f, 0.2943f, 0.7185f, 0.1079f};

    public NNModel modelAsset; // Drag your .onnx file here in the inspector
    private IWorker worker;
    private Model model;
    private int batch = 1;
    private int channel = 1;
    private int height = 6;
    private int width = 3;

    public static float[] resultArray = new float[4] {0f, 0f, 0f, 0f};

    void Start()
    {
        // Load the model and create a worker for inference
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
                    // Reorder data: NHWC
                    reorderedInput[h * width * channels + w * channels + c] =
                        inputArray[c * height * width + h * width + w];
                }
            }
        }

        return reorderedInput;
    }

    public float[] RunModel(float[] inputArray)
    {
        // Prepare the input tensor
        Tensor inputTensor = new Tensor(batch, height, width, channel, ReorderInput(inputArray));

        // Execute the model
        worker.Execute(inputTensor);

        // Get the output tensor
        Tensor outputTensor = worker.PeekOutput();
        float[] outputArray = outputTensor.ToReadOnlyArray();

        // Dispose tensors to avoid memory leaks
        inputTensor.Dispose();
        outputTensor.Dispose();

        resultArray = outputArray;

        return outputArray;
    }

    void OnDestroy()
    {
        worker.Dispose();
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     var outputA = RunModel(dummyArray);
        //     Debug.Log(outputA[0]);
        //     Debug.Log(outputA[1]);
        //     Debug.Log(outputA[2]);
        //     Debug.Log(outputA[3]);
        // }
    }
}
