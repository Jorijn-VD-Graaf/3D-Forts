using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostText : MonoBehaviour
{
    public GameObject attachedObject;
    Camera myCamera;

    public float metalDisplay = 0.0f;
    public float energyDisplay = 0.0f;
    Text myText;

    void Awake() {
        myCamera = Camera.main;
        myText = GetComponent<Text>();
    }

   
    void FixedUpdate() {
        if (attachedObject) {
            if (Vector3.Dot(myCamera.transform.forward, attachedObject.transform.position - myCamera.transform.position) > 0) {
                transform.position = myCamera.WorldToScreenPoint(attachedObject.transform.position);
                myText.text = "M:" + metalDisplay + "\nE:" + energyDisplay;
            }
        } else {
            Destroy(gameObject);
        }
    }

    public void RedAffordText(bool canAfford) {
        if (canAfford) {
            myText.color = Color.white;
        } else {
            myText.color = Color.red;
        }
    }
}
