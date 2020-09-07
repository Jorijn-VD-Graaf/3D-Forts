using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStress : MonoBehaviour {
    /*
    This is the main script for platform on calculating their structual strength 
    */

    PanelScript panelScript;
    Rigidbody r;
    float compressionMax = 0.1f;
    float stress;
    float startCompression = 0;

    MeshRenderer meshRenderer;

    AudioClip stressSound;
    AudioClip breakSound;
    AudioSource audioSource;
    Mesh mesh;

    // Use this for initialization
    void Start() {
        r = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        panelScript = GetComponent<PanelScript>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        //Edge Selection
        if (!r.IsSleeping() && startCompression != 0) {
            stress = 1 - CalculateStress(mesh)/startCompression;
        }
        ShowStress(meshRenderer.material.color.a);
        if (stress > compressionMax || stress < -compressionMax) {
            GetComponent<PlatformHealth>().Explode(transform.position, 0);
        }
    }

    float CalculateStress(Mesh m) {
        var triangles = m.triangles;
        var vertices = m.vertices;

        double sum = 0.0;

        for (int i = 0; i < triangles.Length; i += 3) {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            sum += Vector3.Cross(a, b).magnitude;
        }

        return (float)(sum / 2.0);
    }

    public void ShowStress(float alpha) {
        Color stressColor = new Color(1, 1 - (stress * 10), 1 - (stress * 10), alpha);
        panelScript.ChangeColor(stressColor);
    }

    public void RemoveAllLinks() {
        foreach (ConfigurableJoint link in GetComponents<ConfigurableJoint>()) {
            link.breakForce = 0;
        }
    }

    public void SetStartArea() {
        mesh = GetComponent<MeshFilter>().mesh;
        startCompression = CalculateStress(mesh);
    }

    private void OnDestroy() {
        RemoveAllLinks();
    }
}
