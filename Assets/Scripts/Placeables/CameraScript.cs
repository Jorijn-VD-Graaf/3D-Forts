using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraScript : MonoBehaviour {


    private GameObject PowerSlider;
    private GameObject ReloadingUIElement;
    private WarningBoxScript WarningBoxScriptLink;
    public bool IsAttachedToWeapon;
    private GameObject attachedWeapon;
    public float speedH = 4.0f;
    public float speedV = 4.0f;
    public float MoveSpeed = 0.1f;
    private float yaw;
    private float pitch;
    private bool JustAwoke = true;

    public GameObject GetSliderGameobject() {
        return PowerSlider;
    }

    private void Start() {
        PowerSlider = GameObject.Find("PowerSlider");
        ReloadingUIElement = GameObject.Find("ReloadingUI");
        WarningBoxScriptLink = GameObject.Find("Main Warning box").GetComponent<WarningBoxScript>();

        ReloadingUIElement.SetActive(false);
        PowerSlider.SetActive(false);
    }

    private void Update() {
        if (IsAttachedToWeapon) {
            Transform camLoc = attachedWeapon.GetComponent<WeaponScript>().camLoc.transform;
            gameObject.transform.position = camLoc.position;
            gameObject.transform.rotation = camLoc.rotation;
        } else {
            if (Input.GetMouseButton(1)) {
                yaw += speedH * Input.GetAxis("Mouse X");
                pitch -= speedV * Input.GetAxis("Mouse Y");
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
            if (Input.GetKey("w")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position + transform.forward * MoveSpeed;
            }
            if (Input.GetKey("s")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position - transform.forward * MoveSpeed;
            }
            if (Input.GetKey("a")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position - transform.right * MoveSpeed;
            }
            if (Input.GetKey("d")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position + transform.right * MoveSpeed;
            }
            if (Input.GetKey("left ctrl")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position - transform.up * MoveSpeed;
            }
            if (Input.GetKey("space")) {
                GetComponent<Transform>().position = GetComponent<Transform>().position + transform.up * MoveSpeed;
            }
            if (Input.GetKey("left shift")) {
                MoveSpeed = Mathf.Lerp(0.1f, 0.8f, 0.1f);
            } else {
                MoveSpeed = 0.1f;
            }
        }
    }

    public void AttachCamera(GameObject weapon) {
        attachedWeapon = weapon;
        PowerSlider.SetActive(true);
        ReloadingUIElement.SetActive(true);
        attachedWeapon.GetComponent<WeaponScript>().SetCameraAttached(true);
        IsAttachedToWeapon = true;
    }

    public void UnAttachCamera() {
        PowerSlider.SetActive(false);
        attachedWeapon.GetComponent<WeaponScript>().SetCameraAttached(false);
        ReloadingUIElement.SetActive(false);
        attachedWeapon = null;
        IsAttachedToWeapon = false;
    }
}
