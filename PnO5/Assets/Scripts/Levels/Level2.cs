using UnityEngine;
using RoadArchitect;
using Luminosity.IO;

namespace RVP
{
    public class Level2 : Level
    {
        public float speedLimit = 50f;

        Transform car;
        VehicleParent vp;
        WegcodeController wc;
        private SceneLoader sl;
        private float timer;
        private float timeout = 5f;
        private bool detectedRoad;
        private bool detectedInter;
        private float startCooldown;
        private bool detectedStopSign;
        private bool drivingFastEnough;
        private bool stoppedForFirstSign;
        private bool stoppedAtIntersection;
        private Road road;

        private SplineN closestNodeInFront(Road road)
        {
            if (road == null) { return null; }
            SplineN closestNode = null;
            float mindist = Mathf.Infinity;
            float dist;

            foreach (SplineN node in road.spline.nodes)
            {
                float angle = Vector2.Angle(new Vector2((node.transform.position - car.position).x, (node.transform.position - car.position).z), new Vector2(car.forward.x, car.forward.z));
                if (angle > 90)
                {
                    continue;
                }
                dist = Vector2.Distance(new Vector2(node.transform.position.x, node.transform.position.z), new Vector2(car.position.x, car.position.z));
                if (dist < mindist)
                {
                    closestNode = node;
                    mindist = dist;
                }
            }

            return closestNode;
        }

        

        void Start()
        {
            levelCompletedHUD.SetActive(false);
            levelFailedHUD.SetActive(false);
            sl = new();
            startCooldown = 1f;
            drivingFastEnough = false;
            stoppedForFirstSign = false;
            stoppedAtIntersection = false;
            detectedInter = false;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            detectedRoad = false;
            road = null;
            detectedInter = false;
            detectedStopSign = false;
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
                    if (parent && parent.name.StartsWith("Road") && !parent.name.StartsWith("RoadSign") && dist < mindist)
                    {
                        Transform roadtransform = parent.parent.parent;
                        road = roadtransform.gameObject.GetComponent<Road>();
                        road.roadSpeedLimit = 50f;
                        wc.speedLimit = 50f;
                        mindist = dist;
                        detectedRoad = true;
                    }
                    if (parent.name.StartsWith("Inter"))
                    {
                        detectedRoad = true;
                        detectedInter = true;
                    }
                }

                hitColliders = Physics.OverlapSphere(car.position, 10f);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.name.StartsWith("Stop"))
                    {
                        detectedStopSign = true;
                    }
                    else if (hitCollider.gameObject.transform.parent && hitCollider.gameObject.transform.parent.name.StartsWith("Road") && !hitCollider.gameObject.transform.parent.name.StartsWith("RoadSign"))
                    {
                        mntRoads++;
                    }
                }

                if (timer <= 0 && startCooldown <= 0)
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
                            signals.Add("You were driving against traffic!");
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
                        timer = timeout;

                        levelFailedDescription.text = "";
                        foreach (string descr in signals)
                        {
                            levelFailedDescription.text += descr + "\n";
                        }
                        signals = new();
                    }

                    // Should have stopped for Sign AND the intersection and be driving
                    else if (stoppedForFirstSign && stoppedAtIntersection && InputManager.GetAxis("Accel") > -0.5f && InputManager.GetAxis("Brake") == -1 && vp.velMag * 2.23694f * 1.609344f > 10f)
                    {
                        levelCompletedHUD.SetActive(true);
                        if (!drivingFastEnough)
                        {
                            levelCompletedDescription.text = "Just try to drive closer to the speedlimit next time.\n Other drivers don't appreciate slugs!";
                        }
                        timer = timeout;
                    }

                    // Checkpoint 1: Stopsign
                    else if (wc.Car_ShouldStop && wc.Car_HasStopped && !stoppedForFirstSign && detectedStopSign)
                    {
                        stoppedForFirstSign = true;
                    }

                    // Checkpoint 2: Intersection
                    else if (wc.Car_ShouldStop && wc.Car_HasStopped && !stoppedAtIntersection && detectedInter)
                    {
                        stoppedAtIntersection = true;
                    }


                    if (vp.velMag * 2.23694f * 1.609344f >= speedLimit * 0.9f && vp.velMag * 2.23694f * 1.609344f <= speedLimit * 1.1f)
                    {
                        drivingFastEnough = true;
                    }
                }

                else if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                        sl.LoadScene("menu_levels");
                }

                else
                {
                    startCooldown -= Time.deltaTime;
                }
            }
        }
    }
}
