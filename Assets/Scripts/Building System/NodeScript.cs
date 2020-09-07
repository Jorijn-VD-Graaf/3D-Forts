using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour
{
    public GameObject nodeConnector;
    public bool isFoundation = false;

    private void Awake() {
        nodeConnector = gameObject;
    }

    private void OnTriggerStay(Collider collider) {
        GameObject other = collider.gameObject;
        if (other.tag == "Node") {
            nodeConnector = other;
        }
        if (other.name == "Foundation") {
            isFoundation = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject == nodeConnector) {
            nodeConnector = gameObject;
        }
        if (other.name == "Foundation") {
            isFoundation = false;
        }
    }
}
