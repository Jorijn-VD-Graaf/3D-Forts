using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSystem : MonoBehaviour {
    /*
    THIS IS THE MAIN BUILDING SCRIPT FOR THE PLAYER
    Manages:
    building sounds,
    extrusions,
    snapping for plats,
    and repairing
    */

    //Materials
    public Material panelMat;
    public Material panelGhost;

    //Raycast Planes
    public GameObject walls;

    //Sounds
    public AudioClip hammerSound;
    public AudioClip trashSound;
    public AudioClip repairSound;
    AudioSource audioSource;

    //Repairing Stuff
    GameObject repairZone;
    public Image repairCircle;
    Animator repairFluxer;
    RepairZone repairZoneScript;

    //Cost Display
    public GameObject progressBar;
    public GameObject costText;
    GameObject myCostText;
    CostText costTextScript;
    GameObject canvas;

    //Placing limits
    float maxDist = 0.75f;
    float minDist = 0.05f;
    float snap = 0.1f;

    //The mouse dragging stuff    
    bool dragging = false;

    //Resource Manager
    DataManager resourceManager;

    public GameObject hitObj;
    Ray mouseRay;
    RaycastHit hitInfo;

    public List<GameObject> startingNodes = new List<GameObject>();

    List<GameObject> platforms = new List<GameObject>();

    public GameObject nodePrefab;
    List<GameObject> ghostNodes = new List<GameObject>();

    Vector3 mousePos = Vector3.zero;
    Vector3 mousePreviousPos = Vector3.zero;
    Vector3 mouseHitLoc = Vector3.zero;

    private void OnGUI() {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        mousePos = Input.mousePosition;
        mousePreviousPos = mousePos;
    }

    private void Awake() {
        Construct(BuildPanel(startingNodes));
        audioSource = GetComponent<AudioSource>();
        repairZone = transform.GetChild(0).gameObject;
        repairZoneScript = repairZone.GetComponent<RepairZone>();
        repairFluxer = repairCircle.GetComponent<Animator>();
        canvas = GameObject.Find("Canvas");
        resourceManager = GameObject.Find("DataManager").GetComponent<DataManager>();
    }

    private void Update() {
        RepairAura();
        if (Physics.Raycast(mouseRay, out hitInfo)) {
            mouseHitLoc = hitInfo.point;
            if (!Input.GetMouseButton(0)) {
                if (!Input.GetMouseButton(0) && hitInfo.collider.gameObject.tag == "Build") {
                    if (hitObj && hitInfo.collider.gameObject != hitObj) {
                        hitObj.GetComponent<PanelScript>().Highlight(false);
                    }
                    hitObj = hitInfo.collider.gameObject;
                    hitObj.GetComponent<PanelScript>().Highlight(true);
                } else if (hitObj) {
                    hitObj.GetComponent<PanelScript>().Highlight(false);
                    hitObj = null;
                }
            }
            if (hitObj) {
                if (Input.GetMouseButtonDown(0)) {
                    CreateExtrusion(hitObj);
                } else if (Input.GetMouseButton(0)) {
                    //UpdateExtrusion(hitObj);
                }
                if (Input.GetKeyDown("q")) {
                    hitObj.GetComponent<PanelScript>().StartDestruction(progressBar);
                    //Play Trash sound
                    if (audioSource && !audioSource.isPlaying) {
                        audioSource.clip = trashSound;
                        audioSource.Play();
                    }
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                if (ghostNodes.Count > 0 && resourceManager.CanAfford(50.0f, 250.0f)) {
                    UpdateNodes();
                    foreach (GameObject plat in platforms) {
                        if (!plat.GetComponent<PanelScript>().CheckInteresecting(0.05f)) {
                            Construct(plat);
                        } else {
                            Destroy(plat);
                        }
                    }
                } else {
                    foreach (GameObject plat in platforms) {
                        Destroy(plat);
                    }
                }
                platforms.Clear();
                /*for (int i = 0; i < ghostNodes.Count; i++) {
                    if (ghostNodes[i] != ghostNodes[0].GetComponent<NodeScript>().nodeConnector) {
                        Destroy(ghostNodes[i]);
                    }
                }*/
                ghostNodes.Clear();
                hitObj = null;
                walls.SetActive(false);
            }
        }
    }

    void CreateExtrusion(GameObject extrudePanel) {
        walls.SetActive(true);
        walls.transform.position = extrudePanel.transform.position;
        walls.transform.rotation = extrudePanel.transform.rotation;
        PanelScript panelScript = extrudePanel.GetComponent<PanelScript>();
        //for the 4 nodes of the selected panel
        float mouseDis = 1.0f;
        Vector3 normal = panelScript.GetNormal();
        for (int i = 0; i < panelScript.nodes.Count; i++) {
            ghostNodes.Insert(i, Instantiate(nodePrefab, panelScript.nodes[i].transform.position, panelScript.nodes[i].transform.rotation));
            ghostNodes[i].transform.position = panelScript.nodes[i].transform.position + (normal * mouseDis);
        }

        //Build Panels
        List<GameObject> ghostNodeConnectors = new List<GameObject>() {
            ghostNodes[0].GetComponent<NodeScript>().nodeConnector,
            ghostNodes[1].GetComponent<NodeScript>().nodeConnector,
            ghostNodes[2].GetComponent<NodeScript>().nodeConnector,
            ghostNodes[3].GetComponent<NodeScript>().nodeConnector,
        };
        platforms.Insert(0, BuildPanel(ghostNodeConnectors));
        platforms.Insert(1, BuildPanel(CreateNodeList(panelScript, 0, 1)));
        platforms.Insert(2, BuildPanel(CreateNodeList(panelScript, 1, 2)));
        platforms.Insert(3, BuildPanel(CreateNodeList(panelScript, 2, 3)));
        platforms.Insert(4, BuildPanel(CreateNodeList(panelScript, 3, 0)));
    }

    void UpdateExtrusion(GameObject extrudePanel) {
        PanelScript panelScript = extrudePanel.GetComponent<PanelScript>();
        for (int i = 0; i < ghostNodes.Count; i++) {
            Vector3 normal = panelScript.GetNormal();
            Vector3 mouseDir = Vector3.Normalize(mouseHitLoc - extrudePanel.transform.position);
            float mouseDis = Mathf.Clamp(Vector3.Distance(mouseHitLoc, extrudePanel.transform.position) * Vector3.Dot(normal, mouseDir), 0.1f, 1.5f);
            ghostNodes[i].transform.position = panelScript.nodes[i].transform.position + (normal * mouseDis);
        }
    }

    List<GameObject> CreateNodeList(PanelScript panelScript, int nodeA, int nodeB) {
        List<GameObject> nodeConnectors = new List<GameObject>() {
            panelScript.nodes[nodeA].GetComponent<NodeScript>().nodeConnector,
            panelScript.nodes[nodeB].GetComponent<NodeScript>().nodeConnector,
            ghostNodes[nodeB].GetComponent<NodeScript>().nodeConnector,
            ghostNodes[nodeA].GetComponent<NodeScript>().nodeConnector,
        };
        return nodeConnectors;
    }

    void UpdateNodes() {
        foreach (GameObject panel in platforms) {
            List<GameObject> nodeConnectors = new List<GameObject>() {
                 panel.GetComponent<PanelScript>().nodes[0].GetComponent<NodeScript>().nodeConnector,
                 panel.GetComponent<PanelScript>().nodes[1].GetComponent<NodeScript>().nodeConnector,
                 panel.GetComponent<PanelScript>().nodes[2].GetComponent<NodeScript>().nodeConnector,
                 panel.GetComponent<PanelScript>().nodes[3].GetComponent<NodeScript>().nodeConnector,
            };
            panel.GetComponent<PanelScript>().AssignNodes(nodeConnectors);
        }
    }

    GameObject BuildPanel(List<GameObject> nodeConnectors) {
        //get locations of the assigned nodes
        Vector3[] vertices = new Vector3[] {
            nodeConnectors[0].transform.position,
            nodeConnectors[1].transform.position,
            nodeConnectors[2].transform.position,
            nodeConnectors[3].transform.position,
        };
        GameObject panel = PanelBuilder.CreatePlane(vertices, true, panelMat, panelGhost);
        panel.GetComponent<PanelScript>().AssignNodes(nodeConnectors);
        return panel;
    }

    void Construct(GameObject panel) {
        panel.GetComponent<PanelScript>().StartConstruction(progressBar);

        //Play Building sound
        if (audioSource && !audioSource.isPlaying) {
            audioSource.clip = hammerSound;
            audioSource.Play();
        }
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
                    if (obj && obj.GetComponent<PlatformHealth>().Repair(progressBar) && !audioSource.isPlaying) {
                        audioSource.clip = repairSound;
                        audioSource.Play();
                    }
                }
            }
        } else if (repairFluxer) {
            //Stop animation
            repairFluxer.SetBool("Repairing", false);
        }
    }
}
