using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{

    public float StartAndMaxHealth;
    public float CurrentHealth;

    void Start()
    {
        CurrentHealth = StartAndMaxHealth;
    }

    void Update()
    {
        if (CurrentHealth >= StartAndMaxHealth) {
            CurrentHealth = StartAndMaxHealth;
        }
        if (CurrentHealth <= 0)
        {
            DestroySelf();
        }
    }

    private void DestroySelf() {
        var SelfWeaponScript = gameObject.GetComponent<WeaponScript>();
        if (SelfWeaponScript != null && SelfWeaponScript.IsCameraAttached() == true) {
            Camera.main.GetComponent<CameraScript>().UnAttachCamera();
        }
        Destroy(gameObject, 0);
    }

    public void DoDamage(float Damage) {
        CurrentHealth -= Damage;
    }
}
