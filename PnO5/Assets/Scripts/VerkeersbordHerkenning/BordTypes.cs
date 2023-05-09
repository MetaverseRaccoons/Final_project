using UnityEngine;
public class BordTypes : MonoBehaviour
{
    static WegcodeController controller;
    public enum BordType
    {

        Stop,
        Turn,
        SpeedLimit,
        RoadType


    }

    public static void gebruikerBordNotificatie(BordType bord, string argument)
    {

        switch (bord)
        {
            case BordType.SpeedLimit:
                {
                    if (argument == "20")
                        controller.setSpeedLimit(20);
                    else if (argument == "30")
                        controller.setSpeedLimit(30);
                    else if (argument == "50")
                        controller.setSpeedLimit(50);
                    else if (argument == "60")
                        controller.setSpeedLimit(60);
                    else if (argument == "70")
                        controller.setSpeedLimit(70);
                    else if (argument == "80")
                        controller.setSpeedLimit(80);
                    else if (argument == "100")
                        controller.setSpeedLimit(100);
                    else if (argument == "120")
                        controller.setSpeedLimit(120);
                    break;
                    
                }
            case BordType.Turn:
                {
                    if (argument == "left")
                        break;
                    else if (argument == "right")
                        break;
                    else
                        break;

                }
                
            case BordType.RoadType:
                {
                    if (argument == "Priority")
                        break;
                    else if (argument == "Yield")
                        break;
                    else
                        break;

                }
            case BordType.Stop:
                {
                    controller.setSpeedLimit(0);
                    break;
                }
            default: break;
        }

    }

    public void Start()
    {
        controller = this.GetComponentInParent<WegcodeController>();
    }
}