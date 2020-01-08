using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataManager : MonoBehaviour {

    public List<PlaceablesData> PlacedPlaceables;

    public float metal = 600;
    float metalLast;
    public float energy = 6000;
    float energyLast;

    public float metalStorage = 600;
    public float energyStorage = 6000;

    public float windFloorTop = 4;
    public float windFloorMiddle = -3;
    public float windFloorBottem = -6;

    public TextMeshProUGUI MetalTextMesh;
    public TextMeshProUGUI EnergyTextMesh;

    private void Start() {
        metalLast = metal;
        energyLast = energy;
    }

    private void FixedUpdate() {
        //Keep the stuff in the limits
        if (metal > metalStorage) {
            metal = metalStorage;
        } else if (metal < 1) {
            metal = 1;
        }
        if (energy > energyStorage) {
            energy = energyStorage;
        } else if (energy < 1) {
            energy = 1;
        }
        //Calculate how much is coming in or out
        float metalRate = (metalLast - metal) / Time.fixedDeltaTime;
        metalLast = metal;
        float energyRate = (energyLast - energy) / Time.fixedDeltaTime;
        energyLast = energy;
        //Display
        MetalTextMesh.text = string.Format("{0}/{1} {2}", Mathf.Round(metal), metalStorage, Mathf.Round(Mathf.Abs(metalRate)));
        EnergyTextMesh.text = string.Format("{0}/{1} {2}", Mathf.Round(energy), energyStorage, Mathf.Round(Mathf.Abs(energyRate)));
    }

    //Subtract a price of something from the current resources.
    public void SubtractPrice(float metalPrice, float energyPrice) {
        metal -= metalPrice;
        energy -= energyPrice;
    }

    //Check if for the price you can afford something.
    public bool CanAfford(float metalPrice, float energyPrice) {
        if (metal >= metalPrice && energy >= energyPrice) {
            return true;
        }
        return false;     
    }
}
