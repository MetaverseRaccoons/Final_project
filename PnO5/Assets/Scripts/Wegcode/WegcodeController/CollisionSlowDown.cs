using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSlowDown : MonoBehaviour
{
    public Level toSignal;

    void OnTriggerEnter(Collider other) {
        if (other.transform.parent.GetComponent("WegcodeController") != null) {
            if (!other.transform.parent.GetComponent<WegcodeController>().carIsSlowingDown()) {
                Debug.Log("INCORRECT: Car was not slowing down!");
                if (toSignal)
                {
                    toSignal.signals.Add("You should be slowing down!");
                }
            }
        }
    }
}
