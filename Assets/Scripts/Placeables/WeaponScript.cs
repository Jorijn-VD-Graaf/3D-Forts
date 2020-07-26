using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponScript : MonoBehaviour {

    private GameObject ReloadingUI;
    private ProjectileManager ProjectileManager;
    private Slider PowerSlider;

    public GameObject projectilePrefab;

    private bool CameraAttached = false;

    //Weapon parts to manage
    public GameObject ZRotator;
    public GameObject weaponBarrel;
    public GameObject shootLoc;
    public GameObject camLoc;

    private bool reloading;

    //Weapon Stats
    public float reloadSpeed = 10;
    public float FireCostEnergy = 1000;
    public float FireCostMetal = 100;

    private float CurrentPower;
    public float MaxPower = 35f;
    public float MinPower = 35f;
    private float yaw;
    private float pitch;

    private WarningBoxScript WarningBox;
    private DataManager DataManager;
    private Animator animator;

    public AudioClip fireSound;
    AudioSource audioSource;

    public bool IsCameraAttached() {
        return CameraAttached;
    }

    public PlaceablesScript GetPlaceableScript() {
        return gameObject.GetComponent<PlaceablesScript>();
    }

    private void Awake() {
        ReloadingUI = GameObject.Find("ReloadingUI");
        ProjectileManager = GameObject.Find("ProjectileManager").GetComponent<ProjectileManager>();
        DataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        WarningBox = GameObject.Find("Main Warning box").GetComponent<WarningBoxScript>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetCameraAttached(bool Attached) {
        CameraScript CameraScript = Camera.main.GetComponent<CameraScript>();
        PowerSlider = CameraScript.GetSliderGameobject().GetComponent<Slider>();
        CameraAttached = Attached;
    }

    public void FireWeapon(Vector3 angle, float power) {
        GameObject projectile = Instantiate(projectilePrefab, shootLoc.transform.position, shootLoc.transform.rotation);
        projectile.GetComponent<Rigidbody>().AddForce(angle * power, ForceMode.VelocityChange);
        audioSource.clip = fireSound;
        audioSource.Play();
        projectile.transform.position = shootLoc.transform.position;
        animator.SetTrigger("Fire");
    }

    private void Reload() {
        reloading = false;
    }

    private void Update() {
        if (CameraAttached) {
            SetPower();
            AimBarrel();
            FireIfReady();
        }
        if (reloading && ReloadingUI != null && ReloadingUI.activeSelf == false) {
            ReloadingUI.SetActive(true);
        } else if (reloading == false && ReloadingUI != null && ReloadingUI.activeSelf == true) {
            ReloadingUI.SetActive(false);
        }
    }

    private void SetPower() {
        CurrentPower = Mathf.Clamp(CurrentPower, MinPower, MaxPower);
        if (Input.mouseScrollDelta.y < 0) {
            CurrentPower += 0.5f;
        }
        if (Input.mouseScrollDelta.y > 0) {
            CurrentPower -= 0.5f;
        }

        CurrentPower = Mathf.Clamp(CurrentPower, MinPower, MaxPower);
        if (Math.Abs(CurrentPower) > Mathf.Epsilon) {
            PowerSlider.value = (CurrentPower) / MaxPower;
        } else {
            PowerSlider.value = 0;
        }
    }

    private void AimBarrel() {
        float AimingSinsitivityX = 1;
        float AimingSinsitivityY = 1;
        Camera mainCamera = Camera.main;
        Vector2 mousePos = new Vector2 {
            x = Camera.main.pixelWidth - (((Camera.main.pixelWidth / 2 - Input.mousePosition.x) * AimingSinsitivityX) + Camera.main.pixelWidth / 2),
            y = Camera.main.pixelHeight - (((Camera.main.pixelHeight / 2 - Input.mousePosition.y) * AimingSinsitivityY) + Camera.main.pixelHeight / 2)
        };
        yaw = AimingSinsitivityX * Input.GetAxis("Mouse X");
        pitch = AimingSinsitivityY * Input.GetAxis("Mouse Y");
        ZRotator.transform.Rotate(0.0f, 0.0f, yaw);
        weaponBarrel.transform.Rotate(pitch, 0.0f, 0.0f);
    }

    private void FireIfReady() {

        if (Input.GetMouseButtonDown(0) && reloading == false && DataManager.CanAfford(FireCostMetal, FireCostEnergy)) {
            reloading = true;
            Invoke("Reload", reloadSpeed);
            DataManager.SubtractPrice(FireCostMetal, FireCostEnergy);
            //Fire the weapon!
            FireWeapon(shootLoc.transform.forward, CurrentPower);

        } else if (Input.GetMouseButtonDown(0) && reloading == true) {
            WarningBox.SetWarning("Cannot Fire, Still Reloading");
        } else if (Input.GetMouseButtonDown(0) && !DataManager.CanAfford(FireCostMetal, FireCostEnergy)) {
            WarningBox.SetWarning("Cannot Fire, Not Enough Resources");
        }
    }
}
