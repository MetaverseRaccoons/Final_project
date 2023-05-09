using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionWithObject : MonoBehaviour
{
    public Level toSignal;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<WegcodeController>() != null)
        {
            Debug.Log("Collision!");
            if (toSignal)
            {
                toSignal.signals.Add("You crashed into " + transform.name + "!");
            }
        }
    }
}

