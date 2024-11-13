import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F


# Define the model architecture
class ADHDDiagnosisModel(nn.Module):
    def __init__(self):
        super(ADHDDiagnosisModel, self).__init__()

        # Convolutional layers
        self.conv1 = nn.Conv2d(in_channels=1, out_channels=10, kernel_size=3, padding=1)
        self.conv2 = nn.Conv2d(
            in_channels=10, out_channels=20, kernel_size=3, padding=1
        )

        # Fully connected layers
        self.fc1 = nn.Linear(
            20 * 6 * 3, 32
        )  # Flattened output of conv layers to 32 neurons
        self.fc2 = nn.Linear(32, 4)  # Output layer for 4 classes

        # Softmax for output
        self.softmax = nn.Softmax(dim=1)

    def forward(self, x):
        # Apply first convolutional layer with ReLU activation
        x = F.relu(self.conv1(x))

        # Apply second convolutional layer with ReLU activation
        x = F.relu(self.conv2(x))

        # Flatten the tensor for fully connected layers
        x = x.view(x.size(0), -1)

        # Fully connected layer with ReLU
        x = F.relu(self.fc1(x))

        # Output layer with softmax
        x = self.softmax(self.fc2(x))

        return x


# Instantiate the model
model = ADHDDiagnosisModel()

# Example input tensor
# Assuming batch size of 1, single-channel (1), and input size of 6x3
example_input = torch.randn(1, 1, 6, 3)  # Batch size of 1
print(example_input)

# Forward pass through the model
output = model(example_input)

print("Output probabilities:", output)


# # Instantiate your model
# model = ADHDDiagnosisModel()

# # Create a dummy input tensor matching the input shape expected by the model
# dummy_input = torch.randn(1, 1, 6, 3)  # Batch size 1, single channel, 6x3 input

# # Export the model to an ONNX file
# torch.onnx.export(
#     model,  # model being run
#     dummy_input,  # example input for tracing
#     "adhd_diagnosis_model.onnx",  # where to save the model
#     export_params=True,  # store the trained parameter weights inside the model file
#     opset_version=11,  # ONNX opset version (11 is generally compatible with Barracuda)
#     do_constant_folding=True,  # optimize the model
#     input_names=["input"],  # the model’s input names
#     output_names=["output"],  # the model’s output names
# )
