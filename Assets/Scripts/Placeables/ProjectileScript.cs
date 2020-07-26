using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    private float FireSpeed;
    private Vector3 FireAngle;
    public float Mass = 10;
    public bool UseGravity = true;
    public float ExpireTime = 10;


    public void SetFireSpeed(float Speed) {
        FireSpeed = Speed;
    }

    public void SetFireAngle(Vector3 Angle)
    {
        FireAngle = Angle;
    }


    private void Start()
    {
        var TheRigidBody = gameObject.GetComponent<Rigidbody>();
        gameObject.transform.eulerAngles = FireAngle;
        TheRigidBody.velocity = transform.forward * FireSpeed;
        TheRigidBody.mass = Mass;
        TheRigidBody.useGravity = UseGravity;
        Destroy(gameObject, ExpireTime);
    }
}
