using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVP;
using TMPro;
using RoadArchitect;

public class Level : MonoBehaviour
{
    public Camera mainCam;
    public GameObject levelCompletedHUD;
    public GameObject levelFailedHUD;
    public TextMeshProUGUI levelCompletedDescription;
    public TextMeshProUGUI levelFailedDescription;

    public HashSet<string> signals = new();


    public (Vector3, Vector3) closestSplinePoint(Road road, Vector3 point)
    {
        float param = road.spline.GetClosestParam(point, false, true);
        return (road.spline.GetSplineValue(param), road.spline.GetSplineValue(param, true));
    }
}
