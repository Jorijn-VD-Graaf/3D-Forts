using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //Looking Stuff
    float mouseY;
    public float speedH = 4.0f;
    public float speedV = 4.0f;
    private float moveSpeed = 0.1f;
    private float yaw;
    private float pitch;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Movement();
    }

    void Movement() {
        //Camera Movement
        if (Input.GetKey("w")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position + transform.forward * moveSpeed;
        }
        if (Input.GetKey("s")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position - transform.forward * moveSpeed;
        }
        if (Input.GetKey("a")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position - transform.right * moveSpeed;
        }
        if (Input.GetKey("d")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position + transform.right * moveSpeed;
        }
        if (Input.GetKey("left ctrl")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position - transform.up * moveSpeed;
        }
        if (Input.GetKey("space")) {
            GetComponent<Transform>().position = GetComponent<Transform>().position + transform.up * moveSpeed;
        }
        if (Input.GetKey("left shift")) {
            moveSpeed = Mathf.Lerp(0.1f, 0.8f, 0.1f);
        } else {
            moveSpeed = 0.1f;
        }
        if (Input.GetMouseButton(1)) {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }
}
