using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlacementController : MonoBehaviour {
    /*
     * Main Script for building weapons and devices
     * 
     * Important concepts to understand:
     * CurrentPlaceableObject is the object that has been selected in the hud, has been instantiated and is following the mouse.
     * CurrentSelectedObject is the object that is highlighted yellow.  This allows the person to delete or control a device/weapon.
     * 
     * These two cannot/should not exist at the same time.
     * 
     */

    [SerializeField]
    public List<GameObject> deviceObjects;
    [SerializeField]
    public List<GameObject> weaponsObjects;
    [SerializeField]
    public List<GameObject> techObjects;
    [SerializeField]
    public List<GameObject> uiTabs;

    List<GameObject> currentObjects;

    public Animator canvasAnimator;
    public List<GameObject> HasTheseTech;
    public List<GameObject> HasThesePlaceables;

    public LayerMask collisionMaskPanels;
    public LayerMask collisionMaskTerrain;
    public LayerMask collisionMaskPlaceables;
    /*
     *Hot Keys 
     */
    [SerializeField]
    private KeyCode selectDeviceHotkey = KeyCode.V;
    [SerializeField]
    private KeyCode selectWeaponHotkey = KeyCode.C;
    [SerializeField]
    private KeyCode selectTechHotkey = KeyCode.X;
    [SerializeField]
    private KeyCode selectExistingHotkey = KeyCode.Mouse0;
    [SerializeField]
    private KeyCode deselectExistingHotkey = KeyCode.Escape;
    [SerializeField]
    private KeyCode deleteHotkey = KeyCode.Q;
    [SerializeField]
    private KeyCode placeHotkey = KeyCode.Mouse0;
    [SerializeField]
    private KeyCode weaponControlHotkey = KeyCode.F;


    private WarningBoxScript WarningBoxScriptLink;
    public GameObject canvas;
    private GameObject currentPlaceableObject;
    private GameObject currentSelectedObject;

    private float mouseWheelRotation;

    private int selectionIndex = 0;
    private int selectedCategory;
    private bool nothingSelected = true;
    //keys for sorting
    private static int DEVICES = 0;
    private static int WEAPONS = 1;
    private static int TECHS = 2;

    private GameObject returnObject;

    Dictionary<int, GameObject> HUD;
    public GameObject HUDItemGridLayoutDevice;
    public GameObject HUDItemGridLayoutWeapon;
    public GameObject HUDItemGridLayoutTech;

    public GameObject HUDItemTemplate;

    private float maxPlaceAngle;

    public bool canPlace;
    bool controlingWeapon;

    public DataManager DataManagerScript;

    private void Start() {
        WarningBoxScriptLink = GameObject.Find("Main Warning box").GetComponent<WarningBoxScript>();
        //Default to devices
        selectedCategory = DEVICES;
        currentObjects = deviceObjects;
        HUD = new Dictionary<int, GameObject>();
        HUD.Add(DEVICES, HUDItemGridLayoutDevice);
        HUD.Add(WEAPONS, HUDItemGridLayoutWeapon);
        HUD.Add(TECHS, HUDItemGridLayoutTech);
        //Instantiate the icons for the tabs
        PutItemsInCategory(DEVICES, deviceObjects);
        PutItemsInCategory(WEAPONS, weaponsObjects);
        PutItemsInCategory(TECHS, techObjects);
    }

    void PutItemsInCategory(int category, List<GameObject> items) {
        //For all the prefabs in list
        foreach (GameObject item in items) {
            //Make Hud gameobject
            GameObject HUDItem = Instantiate(HUDItemTemplate);
            int index = items.IndexOf(item);
            HUDItem.name = "HUD-Item." + item.name + index;
            HUDItem.transform.SetParent(HUD[category].transform);
            HUDItem.transform.localScale = new Vector3(1f, 1f, 1f);
            //Give hud button its function
            HUDItem.GetComponent<Button>().onClick.AddListener(() => SetCategoryIndex(index));
            //If has icon sprite
            PlaceablesData itemData = item.GetComponent<PlaceablesScript>().PlaceablesDataScript;
            if (itemData.HUDSprite != null) {
                HUDItem.GetComponent<Image>().sprite = itemData.HUDSprite;
            }
        }
    }

    void Update() {
        if (!controlingWeapon) {
            if (Input.GetKeyDown(selectExistingHotkey)) {
                SelectObject();
            }
            //Manage the currently held object
            if (currentObjects[selectionIndex] != null) {
                MoveCurrentPlaceableObject();
            }
            //Edit currently held object
            if (currentPlaceableObject != null) {
                RotateWithMouseWheel();
                CheckPlacementValidity();
                ReleaseIfClicked();
                SetColor();
            }
            //check hotkeys
            if (currentSelectedObject) {
                if (Input.GetKeyDown(deleteHotkey)) {
                    DeleteSelected();
                }
            }
            if (Input.GetKeyDown(deselectExistingHotkey)) {
                DeselectCurrentPlaceable();
            }
            //CheckPlaceObjectHotkey();
        }
        CheckWhatTypeSelecting();
        if (Input.GetKeyDown(weaponControlHotkey)) {
            AttachCamToWeapon();
        }
    }

    private void AttachCamToWeapon() {
        Camera mainCamera = Camera.main;
        //If selecting something
        if (currentSelectedObject != null) {
            //If selecting a weapon
            if (currentSelectedObject.GetComponent<WeaponScript>() != null) {
                if (!mainCamera.GetComponent<CameraScript>().IsAttachedToWeapon) {
                    mainCamera.GetComponent<CameraScript>().AttachCamera(currentSelectedObject);
                    DeselectCurrentPlaceable();
                    controlingWeapon = true;
                }
            }
        } else if (mainCamera.GetComponent<CameraScript>().IsAttachedToWeapon) {
            mainCamera.GetComponent<CameraScript>().UnAttachCamera();
            controlingWeapon = false;
        }
    }

    private void CheckWhatTypeSelecting() {
        //For each category in the HUD
        foreach (int category in HUD.Keys) {
            if (selectedCategory == category) {
                HUD[category].SetActive(true);
            } else {
                HUD[category].SetActive(false);
            }
        }
    }

    //Function for the category tabs on the UI
    public void SetToCategoryTab(int category) {
        //Destroy currently held object
        if (currentPlaceableObject) {
            Destroy(currentPlaceableObject);
        }
        selectionIndex = 0;
        selectedCategory = category;
        uiTabs[category].transform.SetSiblingIndex(2);
        if (category == DEVICES) {
            currentObjects = deviceObjects;
            canvasAnimator.Play("SwitchToDevicesTab");
        } else if (category == WEAPONS) {
            currentObjects = weaponsObjects;
            canvasAnimator.Play("SwitchToWeaponsTab");
        } else {
            currentObjects = techObjects;
            canvasAnimator.Play("SwitchToTechTab");
            
        }
    }

    //When a object icon is clicked on hud
    public void SetCategoryIndex(int index) {
        nothingSelected = false;
        selectionIndex = index;
        //Destroy currently held object
        if (currentPlaceableObject || currentSelectedObject) {
            DeselectCurrentPlaceable();
        }
    }

    private void SnapToColliderWithTag(GameObject ToSnap, float SnapRadus, string TagName) {
        if (ToSnap == null) {
            Debug.LogError("Error with snapping");
            return;
        }
        Vector3 center = ToSnap.transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(center, SnapRadus);
        float closestDistance = 999f;
        GameObject closestObject = null;
        for (int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].gameObject.tag == TagName && hitColliders[i].gameObject != ToSnap) {
                if (Vector3.Distance(ToSnap.transform.position, hitColliders[i].transform.position) < closestDistance) {
                    closestDistance = Vector3.Distance(ToSnap.transform.position, hitColliders[i].transform.position);
                    closestObject = hitColliders[i].gameObject;
                }             
            }
        }
        if (closestObject != null) {
            if (closestObject.GetComponent<Collider>() is BoxCollider) {

                Vector3 colliderCenter = closestObject.GetComponent<Collider>().bounds.center;
                Vector3 colliderVector3 = closestObject.GetComponent<Collider>().bounds.size;

                float offset = 0.1f;
                Vector3 colliderVector3right = new Vector3(colliderCenter.x + (colliderVector3.x + offset), center.y, colliderCenter.z);
                Vector3 colliderVector3left = new Vector3(colliderCenter.x - (colliderVector3.x + offset), center.y, colliderCenter.z);
                Vector3 colliderVector3front = new Vector3(colliderCenter.x, center.y, colliderCenter.z + (colliderVector3.z + offset));
                Vector3 colliderVector3back = new Vector3(colliderCenter.x, center.y, colliderCenter.z - (colliderVector3.z + offset));

                Vector3[] Sides = new Vector3[4];
                Sides[0] = colliderVector3right;
                Sides[1] = colliderVector3left;
                Sides[2] = colliderVector3front;
                Sides[3] = colliderVector3back;

                float[] Distances = new float[4];
                for (int i = 0; i < 4; i++) {
                    Distances[i] = Vector3.Distance(center, Sides[i]);
                }

                float value = 999f;
                int index = -1;

                foreach (float distance in Distances) {

                }
                for (int i = 0; i < Distances.Length; i++) {
                    if (Distances[i] < value) {
                        index = i;
                        value = Distances[i];
                    }
                }
                ToSnap.transform.position = Sides[index];
                mouseWheelRotation = 0;
            }
        }
    }

    private void DeleteSelected() {
        if (currentSelectedObject) {
            PlaceablesScript DeviceMainScript = currentSelectedObject.GetComponent<PlaceablesScript>();
            PlaceablesData DeviceDataScript = currentSelectedObject.GetComponent<PlaceablesScript>().PlaceablesDataScript;
            DeviceMainScript.InvokeDelete();
            ColorManager colorManager = currentSelectedObject.GetComponent<ColorManager>();
            colorManager.ResetColor();
            currentSelectedObject = null;
        }
    }

    private void ReleaseIfClicked() {
        canPlace = CheckPlacementValidity();
        if (Input.GetKeyDown(placeHotkey)) {            
            PlaceablesScript PlaceableScript = currentPlaceableObject.GetComponent<PlaceablesScript>();
            PlaceablesData DeviceDataScript = PlaceableScript.PlaceablesDataScript;

            if (canPlace && currentPlaceableObject.activeSelf) {
                FixedJoint joint = currentPlaceableObject.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = returnObject.GetComponent<Rigidbody>();
                PlaceableScript.PlaceablesManagerScript = DataManagerScript;
                PlaceableScript.PlacementControllerScript = this;
                PlaceableScript.InvokePlacement();
                HasThesePlaceables.Add(currentPlaceableObject);
                currentPlaceableObject = null;
            }
        }
    }

    private bool CheckPlacementValidity() {
        PlaceablesScript PlaceableScript = currentPlaceableObject.GetComponent<PlaceablesScript>();
        PlaceablesData DeviceDataScript = PlaceableScript.PlaceablesDataScript;
        //Is object colliding with another?
        if (returnObject != null && currentPlaceableObject != null) {
            if (PlaceableScript.isColliding == false) {
                float returnObjectAngleX = returnObject.transform.eulerAngles.x;
                returnObjectAngleX = (returnObjectAngleX > 180) ? returnObjectAngleX - 360 : returnObjectAngleX;

                float returnObjectAngleZ = returnObject.transform.eulerAngles.z;
                returnObjectAngleZ = (returnObjectAngleZ > 180) ? returnObjectAngleZ - 360 : returnObjectAngleZ;

                if (!(maxPlaceAngle >= Mathf.Abs(returnObjectAngleX) && maxPlaceAngle >= Mathf.Abs(returnObjectAngleZ))) {
                    return false;
                }
            } else {
                return false;
            }
        }

        //Does the player have enough funds?
        if (!DataManagerScript.CanAfford(DeviceDataScript.MetalCost, DeviceDataScript.EnergyCost)) {
            WarningBoxScriptLink.SetWarning("Cannot Place, Not Enough Resources");
            return false;
        }
        //Does the player have the required tech?
        if (DeviceDataScript.RequiresTech != "") {
            if (!HasTheseTech.Any(t => t.name == DeviceDataScript.RequiresTech + "(Clone)")) {
                WarningBoxScriptLink.SetWarning("Cannot Place, Player Does Not Have Required Technology");
                return false;
            }
        }
        //Does the player already have the maximum amount?
        if (DeviceDataScript.LimitOfLikePlaceables != 0) {
            int AmountOfLike = 0;
            foreach (GameObject ThePlacables in HasThesePlaceables) {
                if (ThePlacables != null) {
                    if (ThePlacables.name == currentPlaceableObject.name) {
                        AmountOfLike++;
                    }
                }
            }
            if (AmountOfLike >= DeviceDataScript.LimitOfLikePlaceables) {
                WarningBoxScriptLink.SetWarning("Cannot Place, Player Already Has The Max Amount Of This Placeable");
                return false;
            }
        }
        return true;
    }

    private void SetColor() {
        if (currentSelectedObject != null) {
            ColorManager colorManager = currentSelectedObject.GetComponent<ColorManager>();
            colorManager.ChangeColor(Color.yellow);
        }
        if (currentPlaceableObject != null) {
            ColorManager colorManager = currentPlaceableObject.GetComponent<ColorManager>();
            if (!canPlace) {
                colorManager.ChangeColor(new Color(2f, 0f, 0f));
            } else {
                colorManager.ResetColor();
            }
        }
    }

    private void RotateWithMouseWheel() {
        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 45f);
    }

    private void MoveCurrentPlaceableObject() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        //selecting something
        if (!nothingSelected) {
            //If not a mine or silo
            if (!currentObjects[selectionIndex].GetComponent<PlaceablesScript>().PlaceablesDataScript.IsGroundOnly) {
                if (Physics.Raycast(ray, out hitInfo, 999f, collisionMaskPanels)) {
                    returnObject = hitInfo.collider.gameObject;
                    Vector3 placementLoc = new Vector3(returnObject.transform.position.x, returnObject.transform.position.y + returnObject.transform.localScale.y / 2, returnObject.transform.position.z);
                    if (currentPlaceableObject) {
                        currentPlaceableObject.SetActive(true);
                        currentPlaceableObject.transform.position = placementLoc;
                        currentPlaceableObject.transform.rotation = returnObject.transform.rotation;
                    } else {
                        CreatePlaceableObject(currentObjects[selectionIndex], placementLoc);
                    }
                } else if (currentPlaceableObject) {
                    currentPlaceableObject.SetActive(false);
                }
            } else if (Physics.Raycast(ray, out hitInfo, 999f, collisionMaskTerrain)) {
                returnObject = hitInfo.collider.gameObject;
                Vector3 placementLoc = new Vector3(hitInfo.point.x, returnObject.transform.position.y + returnObject.transform.localScale.y / 2 + 0.1f, hitInfo.point.z);
                if (currentPlaceableObject) {
                    currentPlaceableObject.SetActive(true);
                    currentPlaceableObject.transform.position = placementLoc;
                    currentPlaceableObject.transform.rotation = returnObject.transform.rotation;
                    //Needs work
                    SnapToColliderWithTag(currentPlaceableObject, 1.8f, "Snappable");
                } else {
                    CreatePlaceableObject(currentObjects[selectionIndex], placementLoc);
                }
            } else if (currentPlaceableObject) {
                currentPlaceableObject.SetActive(false);
            }
        }
    }

    private void DeselectCurrentPlaceable() {
        //If selected an object
        if (currentSelectedObject != null) {
            ColorManager colorManager = currentSelectedObject.GetComponent<ColorManager>();
            colorManager.ResetColor();
            currentSelectedObject = null;
        } else {
            //If just holding a placeable
            selectionIndex = 0;
            nothingSelected = true;
            if (currentPlaceableObject) {
                Destroy(currentPlaceableObject);
            }
        }
    }

    private void SelectObject() {
        if (!currentPlaceableObject) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 999f, collisionMaskPlaceables)) {
                //Don't immediately select the just placed object
                if (hitInfo.collider.gameObject != currentPlaceableObject) {
                    DeselectCurrentPlaceable();
                    currentSelectedObject = hitInfo.collider.gameObject;
                    ColorManager colorManager = currentSelectedObject.GetComponent<ColorManager>();
                    colorManager.ChangeColor(Color.yellow);
                    selectionIndex = 0;
                    nothingSelected = true;
                    if (currentPlaceableObject) {
                        Destroy(currentPlaceableObject);
                    }
                }
            }
        }
    }

    private void CreatePlaceableObject(GameObject PlaceableObject, Vector3 location) {
        currentPlaceableObject = Instantiate(PlaceableObject, location, Quaternion.identity);
        PlaceablesData DeviceDataScript = currentPlaceableObject.GetComponent<PlaceablesScript>().PlaceablesDataScript;
        maxPlaceAngle = DeviceDataScript.MaxUpAngle;
    }
}