using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ModelController : MonoBehaviour
{
    public Camera cam;
    private List<Transform> detectedSigns = new();
    public int minFrames = 5;

    [SerializeField]
    public Vector3 AICamPos;

    int frameCounter = 0;
    Transform sign;

    public float timeout = 2f;
    private Dictionary<Transform, float> recentlyEvaluated = new();

    public void Start()
    {
        AICamPos = new Vector3(-3, 6, 6.4f);
    }

    void Update()
    {
        // Reduces the timeout timer for each recentlyEvaluated roadsign
        foreach (Transform tr in recentlyEvaluated.Keys.ToList())
        {
            recentlyEvaluated[tr] = recentlyEvaluated[tr] - Time.deltaTime;
        }
        // Remove from timeout the roadsigns that completed their timer
        recentlyEvaluated = recentlyEvaluated.Where(i => i.Value > 0).ToDictionary(i => i.Key, i => i.Value);

        if (detectedSigns.Count == 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.transform.parent && hitCollider.gameObject.transform.parent.name.StartsWith("RoadSigns") && !recentlyEvaluated.ContainsKey(hitCollider.gameObject.transform))
                {
                    detectedSigns.Add(hitCollider.gameObject.transform);
                    recentlyEvaluated[hitCollider.gameObject.transform] = timeout;
                }
            }
        }
        else
        {
            // Chooses new Sign if previous Sign has been processed, Otherwise decrements timer
            if (frameCounter <= 0)
            {
                sign = detectedSigns[0];
                Vector2 forwardDirCar = new Vector2(transform.forward.x, transform.forward.z);
                Vector2 posCar = new Vector2(transform.position.x, transform.position.z);
                Vector2 forwardDirSign = new Vector2(sign.forward.x, sign.forward.z);
                Vector2 posSign = new Vector2(sign.position.x, sign.position.z);

                if (Vector2.Angle(forwardDirCar, forwardDirSign) > 130 && Vector2.Angle(posSign - posCar, forwardDirSign) > 130)
                {
                    frameCounter = minFrames;
                }
                else
                {
                    detectedSigns.RemoveAt(0);
                    return;
                }
            }
            else
            {
                frameCounter--;
            }

            // If timer still running, keep camera at the roadsign. Otherwise, return it to the car and disable it
            if (frameCounter > 0)
            {
                cam.transform.parent = sign;
                cam.transform.localPosition = AICamPos;
                cam.transform.localEulerAngles = new Vector3(3, 180-4.127f, 0.247f);
                cam.transform.gameObject.SetActive(true);
                cam.enabled = true;
            }
            else
            {
                cam.enabled = false;
                cam.transform.gameObject.SetActive(false);
                cam.transform.parent = transform;
                cam.transform.localPosition = new Vector3(0, 0, 0);
                detectedSigns.RemoveAt(0);
            }
        }
    }
}
