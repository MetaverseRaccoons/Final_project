using UnityEngine;
using RoadArchitect;

namespace RVP
{
    public class MainLevel : Level
    {
        public float failedDescrTimeout = 3f;

        Transform car;
        VehicleParent vp;
        WegcodeController wc;
        private float speedLimit;
        private float timer;
        private bool detectedRoad;
        private bool detectedInter;
        private float startCooldown;
        private Road road;

        void Start()
        {
            levelFailedHUD.SetActive(false);
            startCooldown = 1f;
            detectedInter = false;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            detectedRoad = false;
            road = null;
            detectedInter = false;
            int mntRoads = 0;

            if (!car)
            {
                car = mainCam.transform.parent;
                if (car)
                {
                    vp = car.GetComponent<VehicleParent>();
                    wc = car.GetComponent<WegcodeController>();
                    vp.brakeIsReverse = false;
                    Transmission trans = car.GetComponentInChildren<Transmission>();
                    trans.automatic = false;
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
                    if (parent && parent.name.StartsWith("Road") && !parent.name.StartsWith("RoadSign"))
                    {
                        mntRoads++;
                        if (dist < mindist)
                        {
                            Transform roadtransform = parent.parent.parent;
                            road = roadtransform.gameObject.GetComponent<Road>();
                            speedLimit = road.roadSpeedLimit;
                            wc.speedLimit = speedLimit;
                            mindist = dist;
                            detectedRoad = true;
                        }
                    }
                    if (parent.name.StartsWith("Inter"))
                    {
                        detectedRoad = true;
                        detectedInter = true;
                    }
                }



                if (startCooldown <= 0)
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

                        if (!detectedInter && Vector2.Angle((cp - carpos), carforward) < 150 && (Vector2.Angle(pt, carforward) < 30 || Vector2.Angle(pt, carforward) > 150) &&
                            Vector2.Dot((cp - carpos).normalized, carright.normalized) > 0.3f)
                        {
                            signals.Add("You are driving against traffic!");
                        }
                    }

                    // Check if driving too fast
                    if (wc && vp.velMag * 2.23694f * 1.609344f > speedLimit * 1.1f)
                        signals.Add("You are driving to fast! Please respect the speedlimit.");

                    // Check if all four wheels grounded
                    if (vp.groundedWheels < 4)
                        signals.Add("You are driving to recklessly! All four wheels should be grounded at all times.");

                    // Check if correctly stopped everywhere
                    if (wc.stoppingViolation)
                        signals.Add("You should stop in front of a stopsign or a stopstrip!");

                    // Check if there have been any signals given for bad behaviour
                    if (signals.Count > 0)
                    {
                        levelFailedHUD.SetActive(true);
                        levelFailedDescription.text = "";
                        foreach (string descr in signals)
                        {
                            levelFailedDescription.text += descr + "\n";
                        }

                        if (timer <= 0)
                        {
                            timer = failedDescrTimeout;
                        }
                    }
                }
                else
                {
                    startCooldown -= Time.deltaTime;
                }

                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                        signals = new();
                }
                else
                {
                    levelFailedHUD.SetActive(false);
                }
            }
        }
    }
}
