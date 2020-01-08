using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHealth : MonoBehaviour {
    /*
    This is the main script for platforms on health and health display 
    */


    public float maxHealth = 150;
    public float repairRate = 15.0f;
    public float repairCost = 1.0f;
    float health;

    public float explosionRadius;

    public Texture damage_0;
    public Texture damage_1;
    public Texture damage_2;
    public Texture damage_3;

    Material[] mats;
    Texture currentDamage;

    public GameObject healthBar;
    GameObject myHealthBar;
    GameObject canvas;
    public GameObject fracturedObj;

    bool repairing = false;
    float percent;
    PlatformStrut platformStrut;

    private void Start() {
        health = maxHealth;
        canvas = GameObject.Find("Canvas");
        platformStrut = GetComponent<PlatformStrut>();
    }

    private void FixedUpdate() {
        if (repairing && platformStrut.resourceManager.CanAfford(1 * Time.fixedDeltaTime, 0)) {
            platformStrut.resourceManager.SubtractPrice(1 * Time.fixedDeltaTime, 0);
            Damage(-repairRate * Time.fixedDeltaTime);
            if (myHealthBar) {
                myHealthBar.GetComponent<ProgressBar>().percent = percent;
            } else {
                repairing = false;
            }
        } else {
            Destroy(myHealthBar);
        }
    }

    public bool Damage(float damage) {
        if (damage > 0) {
            repairing = false;
        }
        health -= damage;
        mats = GetComponent<MeshRenderer>().materials;
        percent = health / maxHealth;
        if (percent > 0.99f) {
            currentDamage = damage_0;
        } else if (percent > 0.6f) {
            currentDamage = damage_1;
        } else if (percent > 0.2f) {
            currentDamage = damage_2;
        } else {
            currentDamage = damage_3;
        }
        ShowDamageTexture();
        return health <= 0;
    }

    public void Explode(Vector3 explosionLoc, float force) {
        Destroy(gameObject);
        GameObject fracture =  Instantiate(fracturedObj, transform.position, transform.rotation);
        foreach (Transform t in fracture.transform) {
            Rigidbody r = t.GetComponent<Rigidbody>();
            if (r) {
                r.AddExplosionForce(Random.Range((force - 15f > 0? force - 15f : 0.1f), force + 1.5f), explosionLoc, explosionRadius);
            }
        }
    }

    public void ShowDamageTexture () {
        GetComponent<MaterialManager>().UpdateMats(currentDamage);
    }

    public bool Repair () {
        if (health < maxHealth) {
            if (!myHealthBar) {
                myHealthBar = Instantiate(healthBar, canvas.transform);
            }
            myHealthBar.GetComponent<ProgressBar>().ChangeIcon(2);
            myHealthBar.GetComponent<ProgressBar>().attachedObject = gameObject;
            repairing = true;
            return true;
        } else {
            return false;
        }
    }
}
