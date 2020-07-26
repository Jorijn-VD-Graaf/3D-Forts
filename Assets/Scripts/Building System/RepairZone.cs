using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairZone : MonoBehaviour
{
    // What is the platform currently colliding with?
    List<GameObject> currentCollisions = new List<GameObject>();
    void OnTriggerEnter(Collider col) {
        currentCollisions.Add(col.gameObject);
    }

    void OnTriggerExit(Collider col) {
        currentCollisions.Remove(col.gameObject);
    }

    public List<GameObject> CurrentCollisons() {
        return currentCollisions;
    }
}
