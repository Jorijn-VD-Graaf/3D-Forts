using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelScript : MonoBehaviour {
    public List<GameObject> nodes = new List<GameObject>();
    Mesh mesh;
    BoxCollider meshCollider;

    MeshRenderer m;
    Material[] materials;

    //Materials
    public Material ghostMaterial;
    public Material solidMaterial;

    //Resources
    public float costMetal = 10.0f;
    public float costEnergy = 50.0f;
    public DataManager resourceManager;

    //Selection stuff
    bool currentlySelected = false;
    bool solid = false;
    float currentA;
    bool highlight;

    //Progress bar stuff
    GameObject canvas;
    GameObject myBuildBar;
    float currentTime = 0.0f;
    public float buildTime = 5.0f;
    bool build = false;
    public float destroyTime = 3.0f;
    bool destroy = false;

    //Stat
    public float strength = 1000.0f;
    public List<GameObject> intersectingPanels;
    PlatformStress platformStress;

    private void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<BoxCollider>();
        m = GetComponent<MeshRenderer>();
        materials = GetComponent<MeshRenderer>().materials;
        canvas = GameObject.Find("Canvas");
        resourceManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        solidMaterial = m.material;
        ghostMaterial = m.material;
        intersectingPanels = new List<GameObject>();
        platformStress = GetComponent<PlatformStress>();
    }

    private void Start() {
        
    }

    private void Update() {
        if (nodes.Count > 0) {
            MatchVertices(nodes);
        }
    }

    void FixedUpdate() {
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
            currentTime -= Time.deltaTime * (buildTime / destroyTime);
            if (currentTime <= 0) {
                GameObject.Destroy(gameObject);
                //And Add THE COST
                resourceManager.SubtractPrice(-costMetal / 2, -costEnergy / 2);
                destroy = false;
            }
            myBuildBar.GetComponent<ProgressBar>().percent = currentTime / buildTime;
        }
    }

    void MatchVertices(List<GameObject> nodePositions) {
        mesh.vertices = new Vector3[] {
            transform.InverseTransformPoint(nodePositions[0].transform.position),
            transform.InverseTransformPoint(nodePositions[1].transform.position),
            transform.InverseTransformPoint(nodePositions[2].transform.position),
            transform.InverseTransformPoint(nodePositions[3].transform.position),
        };
    }

    void UpdateCollider() {
        Vector3 oldTransform = gameObject.transform.position;
        Quaternion oldrotation = gameObject.transform.rotation;
        //transform.rotation = Quaternion.identity;
        Bounds bounds = new Bounds(gameObject.transform.position, m.bounds.size);
        meshCollider.size = bounds.size;
        //gameObject.transform.position = oldTransform;
        //gameObject.transform.rotation = oldrotation;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
    }

    public void AssignNodes(List<GameObject> assignedNodes) {
        nodes.Clear();
        for (int i = 0; i < assignedNodes.Count; i++) {
            nodes.Insert(i, assignedNodes[i]);
        }
        platformStress.SetStartArea();
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
        springJointDrive.positionSpring = strength;
        springJointDrive.maximumForce = 99999.9f;

        link1.connectedBody = attachedBody.GetComponent<Rigidbody>();
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

    public void StartConstruction(GameObject buildBar) {
        //And SUBRACT THE COST
        resourceManager.SubtractPrice(costMetal, costEnergy);
        //Make a build bar!
        myBuildBar = Instantiate(buildBar, canvas.transform);
        //Set it to hammer icon
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
        myBuildBar.GetComponent<ProgressBar>().attachedObject = gameObject;
        //Start the timer
        build = true;
        //Enable physics
        for (int i = 0; i < nodes.Count; i++) {
            CreateLink(nodes[i], nodes[i].transform.position);
            //If any are touching the ground, make foundations
            if (!nodes[i].GetComponent<NodeScript>().isFoundation) {
                nodes[i].GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        GetComponent<Rigidbody>().isKinematic = false;
        gameObject.layer = 8;
        UpdateCollider();
    }

    public bool StartDestruction(GameObject buildBar) {
        //Flip it around
        destroy = !destroy;
        build = !destroy;
        if (destroy) {
            //If trashing, start timer and change icon
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
        m.material = solidMaterial;
        GetComponent<Collider>().isTrigger = false;
        platformStress = GetComponent<PlatformStress>();
    }

    void Cancel() {
        myBuildBar.GetComponent<ProgressBar>().ChangeIcon(0);
    }

    public void Highlight(bool boolHighlight) {
        highlight = boolHighlight;
        ChangeColor(Color.white);
    }

    public void ChangeColor(Color color) {
        float alpha = m.material.color.a;
        if (highlight) {
            m.material.color = new Color(1.0f, 0.92f, 0.016f, alpha);
        } else {
            m.material.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Build") {
            intersectingPanels.Add(collision.gameObject);
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>(), true);
        }
    }

    public bool CheckInteresecting(float dotThreshold) {
        foreach (GameObject panel in intersectingPanels) {
            if (Mathf.Abs(Vector3.Dot(GetNormal(), panel.GetComponent<PanelScript>().GetNormal())) > 1.0f - dotThreshold && Mathf.Abs((panel.transform.position - transform.position).magnitude) < dotThreshold) {
                return true;
            }
        }
        return false;
    }

    public Vector3 GetNormal() {
        Plane plane = new Plane(nodes[0].transform.position, nodes[1].transform.position, nodes[2].transform.position);
        Vector3 normal = plane.normal;
        return normal;
    }
}
