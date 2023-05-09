using UnityEngine;
using RoadArchitect;
using Luminosity.IO;

namespace RVP
{
    public class Level1 : Level
    {
        public float speedLimit = 50f;

        Transform car;
        VehicleParent vp;
        WegcodeController wc;
        Transmission trans;
        GearboxTransmission gearbox;
        private SceneLoader sl;
        private float timer;
        private float timeout = 5f;
        private bool detectedRoad;
        private float startCooldown;
        private Road road;

        void Start()
        {
            levelCompletedHUD.SetActive(false);
            levelFailedHUD.SetActive(false);
            sl = new();
            startCooldown = 1f;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            detectedRoad = false;
            road = null;

            if (!car)
            {
                car = mainCam.transform.parent;
                if (car)
                {
                    vp = car.GetComponent<VehicleParent>();
                    vp.brakeIsReverse = false;
                    wc = car.GetComponent<WegcodeController>();
                    trans = car.GetComponentInChildren<Transmission>();
                    trans.automatic = false;
                    if (trans is GearboxTransmission)
                    {
                        gearbox = trans as GearboxTransmission;
                    }
                }
                if (!vp)
                    car = null;
            }
            else
            {
                float mindist = Mathf.Infinity;
                Collider[] hitColliders = Physics.OverlapSphere(car.position, 2f);
                foreach (var hitCollider in hitColliders)
                {
                    float dist = Vector3.SqrMagnitude(car.position - hitCollider.ClosestPoint(car.position));
                    Transform parent = hitCollider.gameObject.transform.parent;
                    if (parent && parent.name.StartsWith("Road") && !parent.name.StartsWith("RoadSign") && dist < mindist)
                    {
                        Transform roadtransform = parent.parent.parent;
                        road = roadtransform.gameObject.GetComponent<Road>();
                        road.roadSpeedLimit = 50f;
                        mindist = dist;
                        detectedRoad = true;
                    }
                }

                if (startCooldown <= 0 && timer <= 0)
                {
                    // Check if driving on road
                    if (!detectedRoad)
                        signals.Add("You should stay on the road while driving!");

                    // Check if driving against traffic
                    if (road)
                    {
                        Vector2 carpos = new(car.position.x, car.position.z);
                        Vector2 carforward = new(car.forward.x, car.forward.z);
                        Vector2 carright = new(car.transform.right.x, car.transform.right.z);

                        (Vector3 closestPoint, Vector3 pointTangent) = closestSplinePoint(road, car.position);
                        Vector2 cp = new(closestPoint.x, closestPoint.z);
                        Vector2 pt = new(pointTangent.x, pointTangent.z);

                        Debug.DrawRay(closestPoint, pointTangent, Color.red);
                        Debug.DrawLine(closestPoint, car.position, Color.blue);

                        if (Vector2.Angle((cp - carpos), carforward) < 150 && (Vector2.Angle(pt, carforward) < 30 || Vector2.Angle(pt, carforward) > 150) &&
                            Vector2.Dot((cp - carpos).normalized, carright.normalized) > 0.3f)
                        {
                            signals.Add("You were driving against traffic!");
                        }
                    }

                    // Check if driving to fast
                    if (wc && vp.velMag * 2.23694f * 1.609344f > speedLimit*1.1f)
                        signals.Add("You are driving to fast! Please respect the speedlimit.");

                    // Check if all four wheels grounded
                    if (vp.groundedWheels < 4)
                        signals.Add("You are driving to recklessly! All four wheels should be grounded at all times.");

                    // Check if there have been any signals given for bad behaviour
                    if (signals.Count > 0)
                    {
                        levelFailedHUD.SetActive(true);
                        timer = timeout;

                        levelFailedDescription.text = "";
                        foreach (string descr in signals)
                        {
                            levelFailedDescription.text += descr + "\n";
                        }
                        signals = new();
                    }
                    // User should be driving in second gear, without driving too fast
                    else
                    {
                        if (InputManager.GetAxis("Accel") > -0.5f && InputManager.GetAxis("Brake") == -1 && vp.velMag * 2.23694f * 1.609344f > 10f && (gearbox.currentGear - 1) == 2)
                        {
                            levelCompletedHUD.SetActive(true);
                            timer = timeout;
                        }
                    }
                }

                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                        sl.LoadScene("menu_levels");
                }

                if (startCooldown > 0)
                {
                    startCooldown -= Time.deltaTime;
                }
            }
        }
    }
}
