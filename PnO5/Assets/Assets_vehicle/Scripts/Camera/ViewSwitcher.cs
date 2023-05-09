using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewSwitcher : MonoBehaviour
{
    public Camera mainCam;
    public Camera topCam;

    public enum CameraView
    {
        main,
        top
    }

    void Start()
    {
        mainCam.enabled = true;
        topCam.enabled = false;
    }

    public void SwitchView(CameraView view)
    {
        switch (view)
        {
            case CameraView.main:
                {
                    mainCam.enabled = true;
                    topCam.enabled = false;
                    break;
                }
            case CameraView.top:
                {
                    mainCam.enabled = false;
                    topCam.enabled = true;
                    break;
                }
            default: break;
        }
    }

    public void SwitchTop()
    {
        mainCam.enabled = !mainCam.enabled;
        topCam.enabled = !topCam.enabled;
    }
}
