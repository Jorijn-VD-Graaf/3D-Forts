using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablesScript : MonoBehaviour {

    public float maxPlaceAngle = 30f;

    

    public bool isColliding;
    public PlaceablesData PlaceablesDataScript;
    public DataManager PlaceablesManagerScript;
    public PlacementController PlacementControllerScript;
    public GameObject WindEffeciencyPrefab;
    private WindEffeciency CurrentWindEffeciencyScript;
    float currentTime = 0.0f;

    //Progress bar stuff
    public GameObject canvas;
    public GameObject buildBar;
    GameObject myBuildBar;
    bool build = false;
    bool destroy = false;

    public bool constructed = false;

    public float FindWindEffeciency() {
        return CurrentWindEffeciencyScript.CheckWindEffiecency();
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "RepairZone")
            isColliding = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name != "RepairZone")
            isColliding = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "RepairZone")
            isColliding = false;
    }

    private void Awake()
    {
        canvas = GameObject.Find("Canvas");
        PlaceablesDataScript = gameObject.GetComponent<PlaceablesData>();
        PlaceablesManagerScript = GameObject.Find("DataManager").GetComponent<DataManager>();

        maxPlaceAngle = PlaceablesDataScript.MaxUpAngle;

        if (PlaceablesDataScript.IsWindBasedEnergyProduction == true)
        {
            var WindGameObject = Instantiate(WindEffeciencyPrefab, new Vector3(transform.position.x, transform.position.y + transform.localScale.y, transform.position.z), Quaternion.identity);
            CurrentWindEffeciencyScript = WindGameObject.GetComponent<WindEffeciency>();
            WindGameObject.transform.parent = gameObject.transform;

        }
    }

    private void FixedUpdate() {
        //Timers
        if (build) {
            destroy = false;
            currentTime += Time.fixedDeltaTime;
            if (currentTime >= PlaceablesDataScript.BuildTime) {
                Construct();
                build = false;
            }
            myBuildBar.GetComponent<ProgressBar>().percent = currentTime / PlaceablesDataScript.BuildTime;
        }
        if (destroy) {
            build = false;
            currentTime -= Time.fixedDeltaTime * (PlaceablesDataScript.BuildTime / (PlaceablesDataScript.BuildTime/2/*DeleteTime*/));
            if (currentTime <= 0) {
                Destroy(gameObject);
                //And Add THE COST
                PlaceablesManagerScript.SubtractPrice(-PlaceablesDataScript.MetalCost / 2, -PlaceablesDataScript.EnergyCost / 2);
                destroy = false;
            }
            myBuildBar.GetComponent<ProgressBar>().percent = currentTime / PlaceablesDataScript.BuildTime;
        }
        if (constructed) {
            PlaceablesManagerScript.SubtractPrice(-PlaceablesDataScript.MetalProduction * Time.fixedDeltaTime, -PlaceablesDataScript.EnergyProduction * Time.fixedDeltaTime);
            //If the platform you are on is destroyed
            if (GetComponent<FixedJoint>() && GetComponent<FixedJoint>().connectedBody == null && PlaceablesDataScript.IsGroundOnly == false) {
                //Remove whatever storage you added.
                PlaceablesManagerScript.metalStorage -= PlaceablesDataScript.MetalStorage;
                PlaceablesManagerScript.energyStorage -= PlaceablesDataScript.EnergyStorage;
                /*
                 *I need to make a script that does a destroy effect for devices. 
                 * 
                 */
                Destroy(gameObject);
            }
        }
    }

    public void InvokeDelete()
    {
        destroy = !destroy;
        build = !destroy;
        if (destroy) {
            //If not trashing, start timer and change icon
            if (!myBuildBar) {
                myBuildBar = Instantiate(buildBar, canvas.transform);
            }
            myBuildBar.GetComponent<ProgressBar>().ChangeIcon(1);
            myBuildBar.GetComponent<ProgressBar>().attachedObject = gameObject;
        } else {
            //If so, cancel the trashing!
            Cancel();
        }
    }

    public void InvokePlacement() {
        PlaceablesManagerScript.SubtractPrice(PlaceablesDataScript.MetalCost, PlaceablesDataScript.EnergyCost);
        myBuildBar = Instantiate(buildBar, canvas.transform);
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
        myBuildBar.GetComponent<ProgressBar>().attachedObject = gameObject;
        build = true;
    }

    void Cancel() {
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
    }

    public void Construct() {

        PlaceablesManagerScript.metalStorage += PlaceablesDataScript.MetalStorage;
        PlaceablesManagerScript.energyStorage += PlaceablesDataScript.EnergyStorage;
        PlaceablesManagerScript.PlacedPlaceables.Add(PlaceablesDataScript);
        if (PlaceablesDataScript.IsTech == true) {
            PlacementControllerScript.HasTheseTech.Add(gameObject);
        }
        constructed = true;
    }

    private void DeleteSelectedDevice()
    {

        PlaceablesManagerScript.SubtractPrice(PlaceablesDataScript.MetalReclaim, PlaceablesDataScript.EnergyReclaim);
        PlaceablesManagerScript.metalStorage -= PlaceablesDataScript.MetalStorage;
        PlaceablesManagerScript.energyStorage -= PlaceablesDataScript.EnergyStorage;
        Destroy(gameObject);
    }
}
