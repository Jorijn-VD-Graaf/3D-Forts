using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour {
    public float metal = 600;
    float metalLast;
    public float energy = 6000;
    float energyLast;
    public GameObject canvas;
    public GameObject resources;
    Text resourceText;

    private void Start() {
        resourceText = resources.GetComponent<Text>();
        metalLast = metal;
        energyLast = energy;
    }

    private void FixedUpdate() {
        float metalRate = (metalLast - metal)/Time.fixedDeltaTime;
        metalLast = metal;
        float EnergyRate = (energyLast - energy)/ Time.fixedDeltaTime;
        energyLast = energy;
        resourceText.text = "Metal:" + Mathf.Round(metal) + " " + Mathf.Round(metalRate) + "+\n Energy:" + Mathf.Round(energy) + " " + Mathf.Round(EnergyRate) + "+";
    }

    public void SubtractPrice (float metalPrice, float energyPrice) {
        metal -= metalPrice;
        energy -= energyPrice;
    }

    public bool CanAfford(float metalPrice, float energyPrice) {
        if (metal >= metalPrice && energy >= energyPrice) {
            return true;
        } else {
            return false;
        }
    }
}
