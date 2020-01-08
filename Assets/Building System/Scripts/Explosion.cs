using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    ParticleSystem p;
    public float areaDamage = 100;

    private void Start() {
        p = GetComponentInChildren<ParticleSystem>();
        foreach (GameObject obj in currentCollisions) {
            if (obj) {
                if (obj.GetComponent<PlatformHealth>().Damage(areaDamage * 1 / Vector3.Distance(transform.position, obj.transform.position))) {
                    obj.gameObject.GetComponent<PlatformHealth>().Explode(transform.position, areaDamage);
                }
            }
        }
    }
    private void FixedUpdate() {
        if (!p) {
            Destroy(gameObject);
        }
    }

    // What is the platform currently colliding with?
    List<GameObject> currentCollisions = new List<GameObject>();
    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Build") {
            currentCollisions.Add(col.gameObject);
        }
    }

    void OnTriggerExit(Collider col) {
        currentCollisions.Remove(col.gameObject);
    }
}
