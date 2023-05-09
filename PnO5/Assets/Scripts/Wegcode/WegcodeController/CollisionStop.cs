using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionStop : MonoBehaviour
{
    bool stopped;

    private void Start()
    {
        stopped = false;
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject component = other.transform.parent.gameObject;

        if (component.name.StartsWith("bumper") && component.transform.parent.parent.GetComponent("WegcodeController") != null)
        {
            GameObject car = component.transform.parent.parent.gameObject;
            car.GetComponent<WegcodeController>().stoppingViolation = false;
            car.GetComponent<WegcodeController>().Car_ShouldStop = true;
            stopped = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject component = other.transform.parent.gameObject;
        if (component.name.StartsWith("bumper_front") && component.transform.parent.parent.GetComponent("WegcodeController") != null )
        {
            GameObject car = component.transform.parent.parent.gameObject;
            if (car.GetComponent<WegcodeController>().Car_HasStopped)
            {
                //Debug.Log("CORRECT: car has stopped");
                car.GetComponent<WegcodeController>().stoppingViolation = false;
                stopped = true;
            }
            else if (car.GetComponent<WegcodeController>().Car_ShouldStop && !stopped)
            {
                //other.transform.parent.GetComponent<WegcodeController>().reportViolation(new Violation { type = Violation.Type.Stop, severity = 1, description = "Did not stop before stop sign" });
                Debug.Log("VIOLATION NIET GESTOPT");
                car.GetComponent<WegcodeController>().stoppingViolation = true;
            }
            car.GetComponent<WegcodeController>().Car_ShouldStop = false;
            car.GetComponent<WegcodeController>().Car_HasStopped = false;
        }
    }
}
