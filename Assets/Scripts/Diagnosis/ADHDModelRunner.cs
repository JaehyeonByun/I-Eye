using Unity.Barracuda;
using UnityEngine;

public class ADHDModelRunner : MonoBehaviour
{
    public NNModel modelAsset; // Drag your .onnx file here in the inspector
    private IWorker worker;
    private Model model;

    void Start()
    {
        // Load the model and create a worker for inference
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }

    public float[] RunModel(float[] inputArray)
    {
        // Prepare the input tensor
        Tensor inputTensor = new Tensor(1, 1, 6, 3, inputArray);

        // Execute the model
        worker.Execute(inputTensor);

        // Get the output tensor
        Tensor outputTensor = worker.PeekOutput();
        float[] outputArray = outputTensor.ToReadOnlyArray();

        // Dispose tensors to avoid memory leaks
        inputTensor.Dispose();
        outputTensor.Dispose();

        return outputArray;
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}
