using UnityEngine;
using RoadArchitect;

namespace RVP
{
    public class Level3 : Level
    {
        public float speedLimit = 50f;
        public Transform referenceCar;

        [SerializeField]
        [Range(1,3)]
        public int stage = 1;

        Transform car;
        VehicleParent vp;
        WegcodeController wc;
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
                        mindist = dist;
                        detectedRoad = true;
                    }
                }

                wc.Car_ShouldStop = true;

                if (startCooldown <= 0 && timer <= 0)
                {
                    // Check if all rules are being followed
                    if (!detectedRoad)
                        signals.Add("You should stay on the road while driving!");

                    // Check if driving too fast
                    if (wc && vp.velMag * 2.23694f * 1.609344f > speedLimit * 1.1f)
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

                    // User should be right behind the first car while standing still
                    else if (Vector2.Angle(new Vector2(referenceCar.gameObject.transform.forward.x, referenceCar.gameObject.transform.forward.z), new Vector2(car.forward.x, car.forward.z)) < 5 
                        && Vector2.Angle(new Vector2(referenceCar.gameObject.transform.position.x, referenceCar.gameObject.transform.position.z) - new Vector2 (car.position.x, car.position.z), new Vector2(car.forward.x, car.forward.z)) < 3 && wc.Car_HasStopped
                        && Vector2.SqrMagnitude(new Vector2(referenceCar.gameObject.transform.position.x, referenceCar.gameObject.transform.position.z) - new Vector2(car.position.x, car.position.z)) < 125f)
                    {
                        levelCompletedHUD.SetActive(true);
                        if (stage != 3)
                            levelCompletedDescription.text = "Well done! Now on to stage " + (stage + 1).ToString() + "!";
                        else
                            levelCompletedDescription.text = "Congratulations! You completed all three stages!";
                        timer = timeout;
                    }
                }

                if (timer > 0)
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        if (levelFailedHUD.activeInHierarchy || stage == 3)
                        {
                            sl.LoadScene("menu_levels");
                        }
                        else
                        {
                            sl.LoadScene("Scenes/Levels/Level3_S-Parkeren_stage" + (stage + 1).ToString());
                        }
                    }
                }

                if (startCooldown > 0)
                {
                    startCooldown -= Time.deltaTime;
                }
            }
        }
    }
}
