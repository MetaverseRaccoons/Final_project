using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionNoEntry : MonoBehaviour
{
    public Level toSignal;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        GameObject car = other.transform.parent.parent.parent.gameObject;
        if (car.GetComponent("WegcodeController") != null && toSignal)
        {
            Debug.Log("Collision");
            toSignal.signals.Add("You should not drive this far!");
        }
    }
}
