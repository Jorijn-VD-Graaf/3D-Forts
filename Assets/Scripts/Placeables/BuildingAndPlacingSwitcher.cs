using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAndPlacingSwitcher : MonoBehaviour
{
    private ExtrudeScript BuildingScriptSystem;
    private PlacementController PlacementScriptSystem;
    private GameObject BuildModeButton;
    private GameObject PlaceableModeButton;


    //When the game starts, get connections to both systems
    private void Start()
    {
        BuildingScriptSystem = gameObject.GetComponent<ExtrudeScript>();
        PlacementScriptSystem = GameObject.Find("PlaceablesPlacementController").GetComponent<PlacementController>();

        BuildModeButton = GameObject.Find("ButtonSystemSwitcherBuilding");
        PlaceableModeButton = GameObject.Find("ButtonSystemSwitcherPlacing");


        SwitchToBuildMode(true);
    }

    public void SwitchToBuildMode(bool button) {
        BuildModeButton.SetActive(button);
        PlaceableModeButton.SetActive(!button);

        BuildingScriptSystem.enabled = button;
        PlacementScriptSystem.enabled = !button;
    }
}
