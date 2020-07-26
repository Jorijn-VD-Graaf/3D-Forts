using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtrudeScript : MonoBehaviour {
    /*
    THIS IS THE MAIN BUILDING SCRIPT FOR THE PLAYER
    Manages:
    building sounds,
    extrusions,
    snapping for plats,
    and repairing
    */

    //Material
    public GameObject wood;

    //Raycast Planes
    public GameObject walls;
    public GameObject walls2;

    //Uh, ghost platforms?
    Dictionary<int, GameObject> ghostPlats;

    //Sounds
    public AudioClip hammerSound;
    public AudioClip trashSound;
    public AudioClip repairSound;
    AudioSource audioSource;

    //Repairng Stuff
    GameObject repairZone;
    public Image repairCircle;
    Animator repairFluxer;
    RepairZone repairZoneScript;

    //Cost Display
    public GameObject costText;
    GameObject myCostText;
    CostText costTextScript;
    public GameObject canvas;

    //Placing limits
    float maxDist = 0.75f;
    float minDist = 0.05f;
    float snap = 0.1f;

    //The mouse dragging stuff    
    Ray ray;
    bool dragging = false;
    GameObject hitObj;
    Vector3 mouseLocation;

    //Resource Manager
    DataManager resourceManager;

    // Use this for initialization
    void Start() {
        ghostPlats = new Dictionary<int, GameObject>();
        audioSource = GetComponent<AudioSource>();
        repairZone = transform.GetChild(0).gameObject;
        repairZoneScript = repairZone.GetComponent<RepairZone>();
        repairFluxer = repairCircle.GetComponent<Animator>();
        canvas = GameObject.Find("Canvas");
        resourceManager = GameObject.Find("DataManager").GetComponent<DataManager>();
    }

    void OnGUI() {
        Vector3 mousePos = Input.mousePosition;
        ray = Camera.main.ScreenPointToRay(mousePos);
        repairCircle.transform.position = mousePos;
    }

    void Update() {

        RepairAura();
        MouseRaycast();

        //While dragging a panel
        if (Input.GetMouseButton(0) && hitObj) {
            //Raycast Planes
            walls.GetComponent<Collider>().enabled = true;
            walls2.GetComponent<Collider>().enabled = true;
            PlatformStrut platformStrut = hitObj.GetComponent<PlatformStrut>();
            //What is the highlighted area? If 4, that means the middle
            if (platformStrut.Highlighted() != 4) {
                Extrude(wood, platformStrut.DragPoint(), mouseLocation, hitObj.transform, 0);
            } else {
                //extrude a plane from each edge.
                foreach (Vector3 edge in platformStrut.edges) {
                    Extrude(wood, edge, mouseLocation, hitObj.transform, platformStrut.edges.IndexOf(edge));
                }
                //create the extra face
                CreateFace(wood, hitObj.transform, 4);
            }
            if (!myCostText) {
                myCostText = Instantiate(costText, canvas.transform);
                costTextScript = myCostText.GetComponent<CostText>();
                costTextScript.attachedObject = ghostPlats[ghostPlats.Count-1];
            }
            costTextScript.metalDisplay = ReturnTotalCostMetal();
            costTextScript.energyDisplay = ReturnTotalCostEnergy();
            costTextScript.RedAffordText(resourceManager.CanAfford(ReturnTotalCostMetal(), ReturnTotalCostEnergy()));
        } else if (ghostPlats.Count > 0) {
            //Just let go
            //Can the player afford building this?
            if (resourceManager.CanAfford(ReturnTotalCostMetal(), ReturnTotalCostEnergy())) {
                ConstructPanels();
            } else {
                //If not, just destroy all the plats
                DestroyPanels();
            }
        } else {
            SelectRollover(MouseRaycast());
        }
    }

    RaycastHit MouseRaycast() {
        //Shoot ray from mouse
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            mouseLocation = hit.point;
        }
        return hit;
    }

    void SelectRollover(RaycastHit hit) {
        //Shoot ray from mouse
        //Aim repair aura
        repairZone.transform.LookAt(hit.point);
        if (hit.rigidbody && hit.rigidbody.gameObject.tag == "Build" && !dragging) {
            //This is the panel being dragged
            if (hitObj && hit.rigidbody != hitObj) {
                hitObj.GetComponent<PlatformStrut>().Selected(hit.point, false);
            }
            //
            //THE MAIN SELECTED PLATFROM
            hitObj = hit.rigidbody.gameObject;
            mouseLocation = hit.point;
            hitObj.GetComponent<PlatformStrut>().Selected(hit.point, true);
            if (Input.GetKeyDown("q")) {
                //Returns true if not already trashing, else cancel the trashing and build instead
                if (hitObj.GetComponent<PlatformStrut>().StartDestruction()) {
                    //If trashing
                    audioSource.clip = trashSound;
                    audioSource.Play();
                } else {
                    //If building
                    audioSource.clip = hammerSound;
                    audioSource.Play();
                }
            }
        } else if (hitObj) {
            hitObj.GetComponent<PlatformStrut>().Selected(hit.point, false);
            hitObj = null;
        }
    }

    void ConstructPanels () {
        //Start construction on all the ghost platforms
        foreach (GameObject obj in ghostPlats.Values) {
            obj.GetComponent<PlatformStrut>().StartConstruction();
            hitObj.GetComponent<PlatformStrut>().Selected(mouseLocation, false);
            //Play Building sound
            if (!audioSource.isPlaying) {
                audioSource.clip = hammerSound;
                audioSource.Play();
            }
        }
        ghostPlats.Clear();
        hitObj = null;
        walls.GetComponent<Collider>().enabled = false;
        walls2.GetComponent<Collider>().enabled = false;
        Destroy(myCostText);
    }

    void DestroyPanels () {
        //Start construction on all the ghost platforms
        foreach (int i in ghostPlats.Keys) {
            Destroy(ghostPlats[i]);
        }
        ghostPlats.Clear();
        hitObj = null;
        walls.GetComponent<Collider>().enabled = false;
        walls2.GetComponent<Collider>().enabled = false;
        Destroy(myCostText);
    }

    void RepairAura() {
        //Do the repair zone thing
        if (Input.GetKey("r") || Input.GetKeyDown("r")) {
            //Animation
            repairFluxer.SetBool("Repairing", true);
            //What are the current collisions in the Zone?
            foreach (GameObject obj in repairZoneScript.CurrentCollisons()) {
                if (obj && obj.tag == "Build") {
                    //If the platform is actually repairing, play the repair sound.
                    if (obj && obj.GetComponent<PlatformHealth>().Repair() && !audioSource.isPlaying) {
                        audioSource.clip = repairSound;
                        audioSource.Play();
                    }
                }
            }
        } else {
            //Stop animation
            repairFluxer.SetBool("Repairing", false);
        }
    }

    void Extrude(GameObject material, Vector3 dragPoint, Vector3 mouseLoc, Transform platTransform, int i) {
        //Haven't already created the face
        if (!ghostPlats.ContainsKey(i)) {
            ghostPlats.Add(i, Instantiate(material, dragPoint, platTransform.rotation));
        }
        //While dragging
        Transform newPlatTransform = ghostPlats[i].transform;
        walls.transform.rotation = newPlatTransform.rotation;
        newPlatTransform.LookAt(dragPoint);
        float rotateAmount = Vector3.SignedAngle(newPlatTransform.up, dragPoint - platTransform.position, newPlatTransform.forward);
        newPlatTransform.transform.Rotate(newPlatTransform.InverseTransformDirection(newPlatTransform.forward), rotateAmount);

        float direction = (platTransform.InverseTransformPoint(mouseLoc)).y/2;
        //Snap it to one
        float snapDistance;
        if (direction > 0.0f && direction < 0.5f + snap) {
            snapDistance = 0.5f;
        } else if (direction < 0.0f && direction > -0.5f - snap) {
            snapDistance = -0.5f;
        } else {
            snapDistance = direction;
        }
        if (direction > 0) {
            if (snapDistance >= minDist) {
                if (snapDistance <= maxDist) {
                    newPlatTransform.position = (dragPoint + (platTransform.up) * (snapDistance));
                    newPlatTransform.localScale = new Vector3(newPlatTransform.localScale.x, newPlatTransform.localScale.y, Mathf.Abs(snapDistance * 2));
                } else {
                    newPlatTransform.position = (dragPoint + platTransform.up * (maxDist + -0.01f));
                }
            } else {
                newPlatTransform.position = (dragPoint + platTransform.up * (minDist + 0.01f));
                newPlatTransform.localScale = new Vector3(newPlatTransform.localScale.x, newPlatTransform.localScale.y, (minDist + 0.01f));
            }
        } else {
            if (snapDistance <= -minDist) {
                if (snapDistance >= -maxDist) {
                    newPlatTransform.position = (dragPoint + (platTransform.up) * (snapDistance));
                    newPlatTransform.localScale = new Vector3(newPlatTransform.localScale.x, newPlatTransform.localScale.y, Mathf.Abs(snapDistance * 2));
                } else {
                    newPlatTransform.position = (dragPoint + platTransform.up * (-maxDist + 0.01f));
                }
            } else {
                newPlatTransform.position = (dragPoint + platTransform.up * (-minDist + -0.01f));
                newPlatTransform.localScale = new Vector3(newPlatTransform.localScale.x, newPlatTransform.localScale.y, (minDist + -0.01f));
            }
        }
    }

    void CreateFace(GameObject material, Transform platTransform, int i) {
        Vector3 createPoint = platTransform.InverseTransformPoint((ghostPlats[0].transform.position + ghostPlats[1].transform.position + ghostPlats[2].transform.position + ghostPlats[3].transform.position)/4);
        if (!ghostPlats.ContainsKey(i)) {
            ghostPlats.Add(i, Instantiate(material, createPoint, platTransform.rotation));
        }
        ghostPlats[i].transform.position = platTransform.TransformPoint(createPoint*2);
        ghostPlats[i].transform.localScale = platTransform.localScale;
    }

    float ReturnTotalCostMetal () {
        float total = 0.0f;
        foreach (GameObject plat in ghostPlats.Values) {
            PlatformStrut platStrut = plat.GetComponent<PlatformStrut>();
            if (!platStrut.IsIntersecting()) {
                total += platStrut.costMetal;
            }
        }
        return total;
    }

    float ReturnTotalCostEnergy() {
        float total = 0.0f;
        foreach (GameObject plat in ghostPlats.Values) {
            PlatformStrut platStrut = plat.GetComponent<PlatformStrut>();
            if (!platStrut.IsIntersecting()) {
                total += platStrut.costEnergy;
            }
        }
        return total;
    }
}
