using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VerkeersbordHUDController : MonoBehaviour
{
    public List<string> verkeersborden = new();
    public Text hud;

    private readonly float minTime = 3f;
    private Dictionary<string, float> internalSigns = new();

    private void Start()
    {
        hud.text = "Roadsigns Detected:\n";
        hud.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        foreach (string bord in verkeersborden)
        {
            internalSigns[bord] = minTime;
        }
        verkeersborden = new();

        hud.text = "Roadsigns Detected:\n";
        foreach (string bord in internalSigns.Keys.ToList())
        {
            hud.text += bord + "\n";
            internalSigns[bord] -= Time.deltaTime;
        }
        
        internalSigns = internalSigns.Where(i => i.Value > 0)
         .ToDictionary(i => i.Key, i => i.Value);

        hud.gameObject.SetActive(true);
        if (internalSigns.Count == 0)
            hud.gameObject.SetActive(false);
    }
}
