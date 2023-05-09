using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Demo Scripts/Vehicle HUD", 1)]

    // Class for the HUD in the demo
    public class VehicleHud : MonoBehaviour
    {
        public GameObject targetVehicle;
        public Text speedText;
        public Text gearText;
        public Slider rpmMeter;
        public Slider boostMeter;
        public Text propertySetterText;
        public Text stuntText;
        public Text scoreText;
        VehicleParent vp;
        Motor engine;
        Transmission trans;
        GearboxTransmission gearbox;
        ContinuousTransmission varTrans;
        StuntDetect stunter;
        public bool stuntMode;
        float stuntEndTime = -1;
        PropertyToggleSetter propertySetter;

        private void Start() {
            if (targetVehicle)
                Initialize(targetVehicle);
        }

        public void Initialize(GameObject newVehicle) {
            if (!newVehicle) { return; }
            targetVehicle = newVehicle;
            vp = targetVehicle.GetComponent<VehicleParent>();

            trans = targetVehicle.GetComponentInChildren<Transmission>();
            if (trans) {
                if (trans is GearboxTransmission) {
                    gearbox = trans as GearboxTransmission;
                }
                else if (trans is ContinuousTransmission) {
                    varTrans = trans as ContinuousTransmission;
                }
            }

            if (stuntMode) {
                stunter = targetVehicle.GetComponent<StuntDetect>();
            }

            engine = targetVehicle.GetComponentInChildren<Motor>();
            propertySetter = targetVehicle.GetComponent<PropertyToggleSetter>();

            if (stuntText)
                stuntText.gameObject.SetActive(stuntMode);

            if (scoreText)
                scoreText.gameObject.SetActive(stuntMode);
        }

        void Update() {
            if (vp) {
                speedText.text = (vp.velMag * 2.23694f * 1.609344f).ToString("0") + " KM/H";

                // If exceeds the speed limit, turn the indicator red
                if (vp.velMag * 2.23694f * 1.609344f > vp.transform.GetComponent<WegcodeController>().speedLimit)
                    speedText.color = Color.red;
                else
                    speedText.color = Color.white;

                if (trans) {
                    if (gearbox) {
                        gearText.text = "Gear: " + (gearbox.currentGear == 0 ? "R" : (gearbox.currentGear == 1 ? "N" : (gearbox.currentGear - 1).ToString()));
                    }
                    else if (varTrans) {
                        gearText.text = "Ratio: " + varTrans.currentRatio.ToString("0.00");
                    }
                }

                if (engine) {
                    rpmMeter.value = engine.targetPitch;

                    if (engine.maxBoost > 0) {
                        boostMeter.value = engine.boost / engine.maxBoost;
                    }
                }

                if (stuntMode && stunter) {
                    stuntEndTime = string.IsNullOrEmpty(stunter.stuntString) ? Mathf.Max(0, stuntEndTime - Time.deltaTime) : 2;

                    if (stuntEndTime == 0) {
                        stuntText.text = "";
                    }
                    else if (!string.IsNullOrEmpty(stunter.stuntString)) {
                        stuntText.text = stunter.stuntString;
                    }

                    scoreText.text = "Score: " + stunter.score.ToString("n0");
                }

                if (propertySetter) {
                    propertySetterText.text = propertySetter.currentPreset == 0 ? "Normal Steering" : (propertySetter.currentPreset == 1 ? "Skid Steering" : "Crab Steering");
                }
            }
        }
    }
}