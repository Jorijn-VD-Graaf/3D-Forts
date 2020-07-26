using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlatformStrut : MonoBehaviour {
    /*
    This is the main script for platform for selecting and building/deleting
    */


    public List<Vector3> edges;
    public List<GameObject> connectedObjs;
    public GameObject strut;

    //Stat
    public float strength = 1000.0f;

    int highlighted = 0;
    Vector3 dragPoint;
    GameObject ghostPlat;

    //Materials
    public Material ghostMaterial;
    public Material selectedMaterial;
    public Material solidMaterial;

    public GameObject canvas;

    //Progress bar stuff
    public GameObject buildBar;
    GameObject myBuildBar;
    float currentTime = 0.0f;
    public float buildTime = 5.0f;
    bool build = false;
    public float destroyTime = 3.0f;
    bool destroy = false;

    MeshRenderer m;
    Material[] materials;
    MaterialManager materialManager;

    //Resources
    public float costMetal = 10.0f;
    public float costEnergy = 50.0f;
    public DataManager resourceManager;

    //Selection stuff
    bool currentlySelected = false;
    bool solid = false;
    float currentA;

    PlatformStress platformStress;

    List<Material> mats;
    Material[] currentMaterials;
    Dictionary<float, int> distances;

    // Use this for initialization
    void Awake() {
        edges = new List<Vector3>();
        mats = new List<Material>();
        distances = new Dictionary<float, int>();
        //What are thy edges?
        edges.Add(transform.TransformPoint(new Vector3(0,0,1) * -transform.localScale.x / 2));
        edges.Add(transform.TransformPoint(new Vector3(0, 0, 1) * transform.localScale.x / 2));
        edges.Add(transform.TransformPoint(new Vector3(1, 0, 0) * -transform.localScale.z / 2));
        edges.Add(transform.TransformPoint(new Vector3(1, 0, 0) * transform.localScale.z / 2));
        m = GetComponent<MeshRenderer>();
        materials = GetComponent<MeshRenderer>().materials;
        foreach (Material mat in m.materials) {
            mats.Add(mat);
        }
        platformStress = GetComponent<PlatformStress>();
        currentlySelected = true;
        canvas = GameObject.Find("Canvas");
        currentMaterials = GetComponent<MeshRenderer>().materials;
        materialManager = GetComponent<MaterialManager>();
        currentA = GetComponent<MeshRenderer>().material.color.a;
        resourceManager = GameObject.Find("DataManager").GetComponent<DataManager>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        //Where are thy edges?
        edges[0] = transform.TransformPoint(new Vector3(0,0,1) * -transform.localScale.x / 2);
        edges[1] = transform.TransformPoint(new Vector3(0, 0, 1) * transform.localScale.x / 2);
        edges[2] = transform.TransformPoint(new Vector3(1, 0, 0) * -transform.localScale.z / 2);
        edges[3] = transform.TransformPoint(new Vector3(1, 0, 0) * transform.localScale.z / 2);
        //Highlight the selected edge/face
        if (highlighted < 4) {
            dragPoint = edges[highlighted];
        } else {
            dragPoint = transform.position;
        }
        //Only show stress if not selected and solid
        if (!currentlySelected && solid) {
            GetComponent<PlatformStress>().ShowStress(currentA);
        }
        //Timers
        if (build) {
            destroy = false;
            currentTime += Time.deltaTime;
            if (currentTime >= buildTime) {
                FinishConstruction();
                build = false;
            }
            myBuildBar.GetComponent<ProgressBar>().percent = currentTime / buildTime;
        }
        if (destroy) {
            build = false;
            currentTime -= Time.deltaTime * (buildTime/destroyTime);
            if (currentTime <= 0) {
                GameObject.Destroy(gameObject);
                //And Add THE COST
                resourceManager.SubtractPrice(-costMetal/2, -costEnergy/2);
                destroy = false;
            }
            myBuildBar.GetComponent<ProgressBar>().percent = currentTime / buildTime;
        }
    }

    public void Selected(Vector3 mouseLoc, bool selected) {
        currentA = GetComponent<MeshRenderer>().material.color.a;
        //If selected
        if (selected) {
            currentlySelected = selected;
            currentMaterials = GetComponent<MeshRenderer>().materials;
            
            //Tells main building script which edges are currently being highlighted.
            if (Vector3.Distance(transform.position, mouseLoc) < 0.4) {
                currentMaterials[0].color = new Color(1, 0.92f, 0.016f, currentA);
                highlighted = 4;
                foreach (Vector3 edge in edges) {
                    currentMaterials[edges.IndexOf(edge) + 1].color = new Color(1, 1, 1, currentA);
                }
            } else {
                currentMaterials[0].color = new Color(1, 1, 1, currentA);

                //Edge Selection
                for (int i = 0; i < edges.Count; i++) {
                    float howFar = Vector3.Distance(edges[i], mouseLoc);
                    distances[howFar] = i;
                }
                float min = 100;
                foreach (float distance in distances.Keys) {
                    if (distance < min) {
                        min = distance;
                    }
                }
                for (int i = 0; i < currentMaterials.Length; i++) {
                    platformStress.ShowStress(currentA);
                }
                highlighted = distances[min];
                currentMaterials[distances[min]+1].color = new Color(1, 0.92f, 0.016f, currentA);
                distances.Clear();
            }
        } else {
            //Go to default color
            for (int i = 0; i < currentMaterials.Length; i++) {
                platformStress.ShowStress(currentA);
            }
        }
    }

    public int Highlighted() {
        return highlighted;
    }

    public Vector3 DragPoint() {
        return dragPoint;
    }

    public void StartConstruction() {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Build")) {
            Physics.IgnoreCollision(GetComponent<Collider>(), obj.GetComponent<Collider>(), true);
        }
        GetComponent<Rigidbody>().useGravity = true;
        //If too near another platform, self-destroy
        if (IsIntersecting()) {
            Destroy(gameObject);
        } else {
            //And SUBRACT THE COST
            resourceManager.SubtractPrice(costMetal, costEnergy);
        }
        foreach (GameObject otherPlat in currentCollisions) {
            if (otherPlat) {
                //If on foundation, make unmovable
                if (transform.position.y < 0.1f) {
                    if (otherPlat.name == "Foundation") {
                        GetComponent<Rigidbody>().isKinematic = true;
                        if (transform.position.y < -0.1f) {
                            Destroy(gameObject);
                            break;
                        }
                    }
                }
                //If colliding with another platform
                if (otherPlat.tag == "Build") {
                    foreach (Vector3 otherEdge in otherPlat.GetComponent<PlatformStrut>().edges) {
                        foreach (Vector3 edge in edges) {
                            //Are any of your edges near the other platforms edges?
                            if (Vector3.Distance(otherEdge, edge) < 0.1f) {
                                //Do you already have a connection to this platform?
                                if (platformStress.AddLink(otherPlat, otherPlat.transform.InverseTransformPoint(edge))) {
                                    //If not, create link
                                    CreateLink(otherPlat, edge);
                                    //and inform the other platform that you're both connected to each other
                                    otherPlat.GetComponent<PlatformStress>().AddLink(gameObject, transform.InverseTransformPoint(otherEdge));
                                }
                            }
                        }
                    }
                }
            }
        }
        //Make a build bar!
        myBuildBar = Instantiate(buildBar, canvas.transform);
        //Set it to hammer icon
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
        myBuildBar.GetComponent<ProgressBar>().attachedObject = gameObject;
        //Start the timer
        currentlySelected = false;
        build = true;
        //Make it selectable
        gameObject.layer = 0;
    }

    public bool StartDestruction() {
        //Flip it around
        destroy = !destroy;
        build = !destroy;
        if (destroy) {
            //If not trashing, start timer and change icon
            if (!myBuildBar) {
                myBuildBar = Instantiate(buildBar, canvas.transform);
            }
            myBuildBar.GetComponent<ProgressBar>().ChangeIcon(1);
            myBuildBar.GetComponent<ProgressBar>().attachedObject = gameObject;
            return true;
        } else {
            //If so, cancel the trashing!
            Cancel();
            return false;
        }
    }

    void FinishConstruction() {
        //Make the material solid, and make it have collisions
        //GetComponent<PlatformStress>().UpdateMaterial(solidMaterial);
        materials = GetComponent<MeshRenderer>().materials;
        for (int i = 0; i < materials.Length; i++) {
            materials[i] = solidMaterial;
        }
        GetComponent<MeshRenderer>().materials = materials;
        GetComponent<Collider>().isTrigger = false;
        currentlySelected = false;
        solid = true;
        gameObject.layer = 8;
    }

    void Cancel() {
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
    }

    ConfigurableJoint CreateLink(GameObject attachedBody, Vector3 connectionPoint) {
        //This creates the configurable joint between platfroms that have near edges.
        ConfigurableJoint link1 = gameObject.AddComponent<ConfigurableJoint>();
        SoftJointLimitSpring springJoint = new SoftJointLimitSpring();
        springJoint.spring = 100.0f;
        springJoint.damper = 1000.0f;

        SoftJointLimit springJointLimit = new SoftJointLimit();
        springJointLimit.limit = 100.0f;

        JointDrive springJointDrive = new JointDrive();
        springJointDrive.positionSpring = (strength + attachedBody.GetComponent<PlatformStrut>().strength)/2;
        springJointDrive.maximumForce = 99999.9f;

        link1.connectedBody = attachedBody.GetComponent<Rigidbody>();
        attachedBody.GetComponent<PlatformStrut>().connectedObjs.Add(gameObject);
        link1.linearLimitSpring = springJoint;
        Vector3 anchorLoc = transform.InverseTransformPoint(connectionPoint);
        link1.anchor = anchorLoc;
        link1.xMotion = ConfigurableJointMotion.Limited;
        link1.yMotion = ConfigurableJointMotion.Limited;
        link1.zMotion = ConfigurableJointMotion.Limited;
        link1.angularXMotion = ConfigurableJointMotion.Free;
        link1.angularYMotion = ConfigurableJointMotion.Free;
        link1.angularZMotion = ConfigurableJointMotion.Free;
        link1.linearLimit = springJointLimit;
        link1.xDrive = springJointDrive;
        link1.yDrive = springJointDrive;
        link1.zDrive = springJointDrive;
        return link1;
    }


    // What is the platform currently colliding with?
    List<GameObject> currentCollisions = new List<GameObject>();
    void OnTriggerEnter(Collider col) {
        currentCollisions.Add(col.gameObject);
    }

    void OnTriggerExit(Collider col) {
        currentCollisions.Remove(col.gameObject);
    }

    public bool IsIntersecting() {
        bool hasElements = currentCollisions.Any(c => c && Vector3.Distance(c.transform.position, transform.position) < 0.1f && c.gameObject.tag == "Build");
        return hasElements;
    }
}
