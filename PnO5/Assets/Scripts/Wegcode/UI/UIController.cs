using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{

    private GameObject Canvas;
    private GameObject TextObject;
    private TMP_Text text;

    private const int waitTimer = 5;
    private double lastUpdate;
    private bool currentlyShowingViolation = false;

    private List<string> violationQueue = new List<string>();

    public void addViolation(Violation violation)
    {
        violationQueue.Add("Violation severity" + violation.severity.ToString() + "\n" + violation.description);
    }

    void Start()
    {
        Canvas = this.gameObject;
        TextObject = Canvas.transform.Find("UIText").gameObject;
        text = TextObject.GetComponent<TMP_Text>();
        lastUpdate = Time.time;
    }

    void Update()
    {
        double newTime = Time.time;
        if (!currentlyShowingViolation)
        {
            if (violationQueue.Count != 0)
            {
                lastUpdate = newTime;
                currentlyShowingViolation = true;
                string pushText = violationQueue[0];
                text.SetText(pushText);
                violationQueue.RemoveAt(0);
            }

        }
        else
        {
            if (newTime - lastUpdate >= waitTimer)
            {
                lastUpdate = newTime;
                currentlyShowingViolation = false;
                text.SetText("");
            }
        }
    }
}