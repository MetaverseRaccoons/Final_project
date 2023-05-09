using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class nn_model2 : MonoBehaviour
{
    // The camera to capture the image from
    public Camera cam;

    // The controller to send the roadsign information to
    public VerkeersbordHUDController hudScript;

    // The neural network model asset
    public NNModel modelAsset;

    // The name of the input tensor
    //public string inputName;

    // The name of the output tensor
    //public string outputName;

    // The size to resize the input image to
    public Vector2Int inputSize;//350 x 350


    // The output tensor shape
    private TensorShape outputShape;

    // The neural network model
    public Model model;

    // The worker for running inference
    private IWorker worker;

    //public Text m_MyText;

    private string[] names;
    //private string new_Text;
    private float temp;
    private int prev;
    private int index;
    private int check;
    private bool send;

    private GameObject ViolationCanvas;


    public void reportViolation(Violation violation)
    {
        ViolationCanvas.GetComponent<UIController>().addViolation(violation);
    }

    private void Start()
    {
        worker = modelAsset.CreateWorker();
        names = new string[]{ "Speed limit (20km/h)", "Speed limit (30km/h)", "Speed limit (50km/h)", "Speed limit (60km/h)", "Speed limit (70km/h)", "Speed limit (80km/h)", "End of speed limit (80km/h)",
        "Speed limit (100km/h)", "Speed limit (120km/h)", "No passing", "No passing for vehicles over 3.5 metric tons", "Right-of-way at the next intersection", "Priority road", "Yield", "Stop",
        "No vehicles", "Vehicles over 3.5 metric tons prohibited", "No entry", "General caution", "Dangerous curve to the left", "Dangerous curve to the right", "Double curve", "Bumpy road", "Slippery road",
        "Road narrows on the right", "Road work", "Traffic signals", "Pedestrians", "Children crossing", "Bicycles crossing", "Beware of ice/snow", "Wild animals crossing", "End of all speed and passing limits",
        "Turn right ahead", "Turn left ahead", "Ahead only", "Go straight or right", "Go straight or left", "Keep right", "Keep left", "Roundabout mandatory", "End of no passing", "End of no passing by vehicles over 3.5 metric tons"};

        ViolationCanvas = GameObject.FindGameObjectWithTag("ViolationCanvas");
    }

    public string Update()
    {
        // Capture the camera image
        var cameraTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 0);
        cam.targetTexture = cameraTexture;
        cam.Render();
        RenderTexture.active = cameraTexture;

        // Resize the image and convert it to a tensor
        var inputTexture = new Texture2D(inputSize.x, inputSize.y);
        inputTexture.ReadPixels(new Rect(0, 0, inputSize.x, inputSize.y), 0, 0);
        inputTexture.Apply();
        var inputTensor = new Tensor(inputTexture, channels: 3);

        // Run inference on the input tensor
        worker.Execute(inputTensor);
        var output = worker.PeekOutput();

        // Process the output tensor
        int count = 1;
        float max = 0;
        float max1 = 0;
        prev = index;
        int runnerUpIdx = 0;

        index = 0;
        for (int i = 0; i < output.length; i++)
        {
            if (max < output[i])
            {
                max1 = max;
                runnerUpIdx = index;
                max = output[i];
                index = i;
            }

            count = count + 1;
        }

        //m_MyText.text = "This is my text";

        if (index == prev)
            { check++; }
        else
            { check = 0; }

        if (max - max1 > 0.5f && max > 3f)
        {
            temp = max - max1;

            Violation violation = new Violation() { type = Violation.Type.MissedRoadsign, severity = 0.6, description = "You missed a roadsign " + names[index] + max };
            if (ViolationCanvas != null)
            {
                reportViolation(violation);
            }
            
            if (hudScript)
            {
                hudScript.verkeersborden.Add(names[index]);
            }
            Debug.Log("You missed a roadsign " + names[index] + " with accuracy " + max + ", " + "Runner Up: " + names[runnerUpIdx] + " with accuracy " + max1);

            send = true;
        }
        else
            {
            //m_MyText.text = "";
            prev = -1;
            send = false;
        }

        // Clean up
        inputTensor.Dispose();
        output.Dispose();
        RenderTexture.ReleaseTemporary(cameraTexture);
        if (send)
        {
            returnBord(name);
            return "You missed a roadsign " + names[index];
        }
        else
            return "";
    }

    public void returnBord(string name)
    {
        if (name.StartsWith("Speed limit"))
        {
            if (name.Contains("20"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "20");
            else if (name.Contains("30"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "30");
            else if (name.Contains("50"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "50");
            else if (name.Contains("60"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "60");
            else if (name.Contains("70"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "70");
            else if (name.Contains("80"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "80");
            else if (name.Contains("100"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "100");
            else
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.SpeedLimit, "120");
        }
        else if (name == "Yield")
            BordTypes.gebruikerBordNotificatie(BordTypes.BordType.RoadType, "Yield");
        else if (name == "Priority road")
            BordTypes.gebruikerBordNotificatie(BordTypes.BordType.RoadType, "Priority");
        else if (name.StartsWith("Turn"))
            if (name.Contains("right"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.Turn, "right");
            else if (name.Contains("Left"))
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.Turn, "left");
        else if(name =="stop")
                BordTypes.gebruikerBordNotificatie(BordTypes.BordType.Stop, "");

    }

    private void OnDestroy()
    {
        // Dispose of the worker
        worker.Dispose();
    }
}