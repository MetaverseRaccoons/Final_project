using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoadArchitect;
using RVP;

public class WegcodeController : MonoBehaviour
{

    public void setSpeedLimit(double limit)
    {
        speedLimit = limit;
    }

    public bool carIsSlowingDown()
    {
        return (currentspeed - previousspeed) < -CARSLOWINGDOWN_BOUND;
    }

    public bool Car_ShouldStop = false;
    public bool Car_HasStopped = false;
    public bool stoppingViolation = false;

    // Car velocity is given in m/s instead of km/h; linear transformation to km/h units
    private static double calculateCurrentSpeed(double rawSpeed)
    {
        return rawSpeed * 3.6f * 0.94f;
    }

    // Handle for the vehicle gameobject
    private GameObject vehicle;
    private VehicleParent vp;
    // Handle for the rigidbody of the vehicle
    private Rigidbody body;
    // Variable constantly updated to current speed of the vehicle
    private double currentspeed;
    // Store the previous speed to determine whether the car is slowing down
    private double previousspeed;
    // contains the speed limit of the road the car is in; vehicle should not exceed this limit
    public double speedLimit = 70;

    // Velocity will never reach zero numerically; speed below this value is considered a full stop
    private const double CARSTOP_VELOCITYBOUND = 0.08f;
    // Determines whether the deceleration of the car is sufficient
    private const double CARSLOWINGDOWN_BOUND = 0.03f;

    private double speedLimitCooldown = 5;
    private double speedLimitViolationTime = 0;

    private GameObject ViolationCanvas;

    public void reportViolation(Violation violation)
    {
        //ViolationCanvas.GetComponent<UIController>().addViolation(violation);
    }


    // Start is called before the first frame update
    void Start()
    {
        vehicle = this.gameObject;
        body = vehicle.GetComponent<Rigidbody>();
        vp = vehicle.GetComponent<VehicleParent>();
        //ViolationCanvas = GameObject.FindGameObjectWithTag("ViolationCanvas");

    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);

        foreach (var hitCollider in hitColliders)
        {
            Transform parent = hitCollider.gameObject.transform.parent;
            if (parent != null && parent.name.StartsWith("Road") && !parent.name.StartsWith("RoadSign"))
            {
                Transform roadtransform = parent.parent.parent;
                Road road = roadtransform.gameObject.GetComponentInParent<Road>();
                setSpeedLimit(road.roadSpeedLimit);
                break;
            }
        }
        // store current speed
        previousspeed = vp.velMag * 2.23694f * 1.609344f;

        if (vp.velMag * 2.23694f * 1.609344f > speedLimit)
        {
            // speed limit exceeded, call method to assign 
            if (Time.time - speedLimitViolationTime >= speedLimitCooldown)
            {
                /*Violation violation = new Violation() { type = Violation.Type.SpeedLimit, severity = 0.95, description = "Speed limit exceeded" };
                ViolationReporter.ReportViolation(violation);
                speedLimitViolationTime = Time.time;
                reportViolation(violation);*/
                Debug.Log("SPEED LIMIT EXCEEDED: " + currentspeed);
            }
        }

        if (Car_ShouldStop && vp.velMag * 2.23694f * 1.609344f < CARSTOP_VELOCITYBOUND)
        {
            Car_HasStopped = true;
        }

    }
}
