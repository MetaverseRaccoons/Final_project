using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Demo Scripts/Vehicle Menu", 0)]

    // Class for the menu in the demo
    public class VehicleMenu : MonoBehaviour
    {
        public Camera AICam;
        public Camera cam;
        public Vector3 spawnPoint;
        public Vector3 spawnRot;
        public GameObject[] vehicles;
        public GameObject chaseVehicle;
        public GameObject chaseVehicleDamage;
        float chaseCarSpawnTime;
        GameObject newVehicle;
        public Toggle autoShiftToggle;
        public Toggle assistToggle;
        public Toggle stuntToggle;
        public VehicleHud hud;

        public bool instantiateOnStart;
        public bool updatedCamera = false;

        private void Start()
        {
            transform.Find("HUD").gameObject.SetActive(false);
            if (instantiateOnStart)
            {
                SpawnVehicle(16);
            }
        }


        void Update() {
            chaseCarSpawnTime = Mathf.Max(0, chaseCarSpawnTime - Time.deltaTime);
            if (!updatedCamera)
            {
                cam.nearClipPlane = 0f;
                cam.nearClipPlane = 0.01f;
                cam.depth = 1;
                updatedCamera = true;
            }
        }

        // Spawns a vehicle from the vehicles array at the index
        public void SpawnVehicle(int vehicle) {
            newVehicle = Instantiate(vehicles[vehicle], spawnPoint, Quaternion.LookRotation(spawnRot, GlobalControl.worldUpDir)) as GameObject;
            cam.transform.parent = newVehicle.transform;
            cam.transform.localEulerAngles = new Vector3(0, 0, 0);
            cam.transform.localPosition = new Vector3(-0.3f, 0.4f, -0.15f);
            AICam.transform.parent = newVehicle.transform;
            AICam.transform.localPosition = new Vector3(0, 0, 0);
            AICam.transform.localEulerAngles = new Vector3(0, 0, 0);
            newVehicle.AddComponent<WegcodeController>();
            newVehicle.AddComponent<BordTypes>();
            newVehicle.AddComponent<ModelController>();
            newVehicle.AddComponent<VehicleHud>();
            ModelController nncontroller = newVehicle.GetComponent<ModelController>();
            nncontroller.cam = AICam;

            if (newVehicle.GetComponent<VehicleAssist>() && assistToggle) {
                newVehicle.GetComponent<VehicleAssist>().enabled = assistToggle.isOn;
            }

            GearboxTransmission geartrans = newVehicle.GetComponentInChildren<GearboxTransmission>();
            geartrans.automatic = false;
            geartrans.startGear = 1;
            geartrans.skipNeutral = false;

            Transmission trans = newVehicle.GetComponentInChildren<Transmission>();
            if (trans && autoShiftToggle) {
                trans.automatic = autoShiftToggle.isOn;
                newVehicle.GetComponent<VehicleParent>().brakeIsReverse = autoShiftToggle.isOn;

                if (trans is ContinuousTransmission && !autoShiftToggle.isOn) {
                    newVehicle.GetComponent<VehicleParent>().brakeIsReverse = true;
                }
            }

            if (newVehicle.GetComponent<FlipControl>() && newVehicle.GetComponent<StuntDetect>() && stuntToggle) {
                newVehicle.GetComponent<FlipControl>().flipPower = stuntToggle.isOn && assistToggle.isOn ? new Vector3(10, 10, -10) : Vector3.zero;
                newVehicle.GetComponent<FlipControl>().rotationCorrection = stuntToggle.isOn ? Vector3.zero : (assistToggle.isOn ? new Vector3(5, 1, 10) : Vector3.zero);
                newVehicle.GetComponent<FlipControl>().stopFlip = assistToggle.isOn;
            }

            if (hud) {
                if (stuntToggle)
                    hud.stuntMode = stuntToggle.isOn;
                hud.Initialize(newVehicle);
            }

            transform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
            transform.parent = cam.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor");
            transform.localPosition = new Vector3(0, 0, 0.3f);
            Transform menu = transform.Find("Menu");
            if (menu)
                menu.gameObject.SetActive(false);
            transform.Find("HUD").gameObject.SetActive(true);
        }

        // Spawns a chasing vehicle
        public void SpawnChaseVehicle() {
            if (chaseCarSpawnTime == 0) {
                chaseCarSpawnTime = 1;
                GameObject chaseCar = Instantiate(chaseVehicle, spawnPoint, Quaternion.LookRotation(spawnRot, GlobalControl.worldUpDir)) as GameObject;
                chaseCar.GetComponent<FollowAI>().target = newVehicle.transform;
            }
        }

        // Spawns a damageable chasing vehicle
        public void SpawnChaseVehicleDamage() {
            if (chaseCarSpawnTime == 0) {
                chaseCarSpawnTime = 1;
                GameObject chaseCar = Instantiate(chaseVehicleDamage, spawnPoint, Quaternion.LookRotation(spawnRot, GlobalControl.worldUpDir)) as GameObject;
                chaseCar.GetComponent<FollowAI>().target = newVehicle.transform;
            }
        }
    }
}