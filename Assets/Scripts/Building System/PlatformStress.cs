using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStress : MonoBehaviour {
    /*
    This is the main script for platform on calculating their structual strength 
    */


    Rigidbody r;
    float compressionMax = 0.1f;
    public Material solid;
    Vector3 stressImpulse;
    float stress;

    float maxAngle = 30;

    Dictionary<GameObject, Vector3> connectedObjs;
    Dictionary<GameObject, float> connectedObjsAngles;
    List<float> compressions;

    float startCompression;
    List<float> distances;

    AudioClip stressSound;
    AudioClip breakSound;
    AudioSource audioSource;

    // Use this for initialization
    void Start() {
        r = GetComponent<Rigidbody>();
        connectedObjs = new Dictionary<GameObject, Vector3>();
        connectedObjsAngles = new Dictionary<GameObject, float>();
        distances = new List<float>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        //Edge Selection
        if (!r.IsSleeping()) {
            stress = CalculateStress();
        }
        if (stress > compressionMax) {
            GetComponent<PlatformHealth>().Explode(transform.position, 0);
        }
    }

    float CalculateStress() {
        foreach (GameObject otherPlat in connectedObjs.Keys) {
            if (otherPlat) {
                //edge for other plat
                Vector3 attachPoint = connectedObjs[otherPlat];
                //angle for other plat
                float attachDirection = connectedObjsAngles[otherPlat];
                //How different is the plat angle since it was attached?
                if (Mathf.Abs(Quaternion.Angle(transform.rotation, otherPlat.transform.rotation) - attachDirection) > maxAngle) {
                    foreach (ConfigurableJoint item in GetComponents<ConfigurableJoint>()) {
                        if (item && item.connectedBody == otherPlat.GetComponent<Rigidbody>()) {
                            otherPlat.GetComponent<PlatformStress>().connectedObjs.Remove(gameObject);
                            Destroy(item);
                        }
                    }
                } else {
                    //How far are you from the attached point?
                    float startDistance = Mathf.Abs(Vector3.Distance(transform.position, transform.transform.TransformPoint(attachPoint)));
                    float currentDistance = Mathf.Abs(Vector3.Distance(transform.position, otherPlat.transform.TransformPoint(attachPoint)));
                    //print("start Distance " + startDistance);
                    //print("current Distance " + currentDistance);
                    distances.Add(currentDistance - startDistance);
                }

            }
        }

        float total = 0;
        foreach (float distance in distances) {
            if (distance > total) {
                total = distance;
            }
        }
        distances.Clear();
        return total;
    }

    public void ShowStress(float alpha) {
        Color stressColor = new Color(1, 1 - (stress * 10), 1 - (stress * 10), alpha);
        GetComponent<MaterialManager>().UpdateColor(stressColor);
    }

    public bool AddLink(GameObject obj, Vector3 connectionPoint) {
        if (!connectedObjs.ContainsKey(obj)) {
            connectedObjs[obj] = connectionPoint;
            connectedObjsAngles[obj] = Quaternion.Angle(transform.rotation, obj.transform.rotation);
            return true;
        } else
            return false;
    }


    public void RemoveAllLinks() {
        foreach (GameObject obj in connectedObjs.Keys) {
            if (obj) {
                obj.GetComponent<PlatformStress>().connectedObjs.Remove(gameObject);
            }
        }
    }

    private void OnDestroy() {
        RemoveAllLinks();
    }
}
