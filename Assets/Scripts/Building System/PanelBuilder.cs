using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelBuilder {
    static int IGNORERAYCAST = 8;

    public static GameObject CreatePlane(Vector3[] points, bool collider, Material mat, Material ghost) {
        GameObject go = new GameObject("Plane");
        Vector3 averagePoint = Vector3.zero;
        foreach (Vector3 point in points) {
            averagePoint += point;
        }
        averagePoint /= points.Length;
        go.transform.position = averagePoint;
        MeshFilter mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        Mesh m = new Mesh();

        m.vertices = new Vector3[] {
            go.transform.InverseTransformPoint(points[0]),
            go.transform.InverseTransformPoint(points[1]),
            go.transform.InverseTransformPoint(points[2]),
            go.transform.InverseTransformPoint(points[3]),
        };

        m.uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };
        m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mf.mesh = m;

        if (collider) {
            go.AddComponent<BoxCollider>();
        }

        mr.material = ghost;

        m.RecalculateNormals();
        m.RecalculateBounds();
        m.Optimize();

        PlatformStress st = go.AddComponent(typeof(PlatformStress)) as PlatformStress;
        PlatformHealth hp = go.AddComponent(typeof(PlatformHealth)) as PlatformHealth;
        PanelScript ps = go.AddComponent(typeof(PanelScript)) as PanelScript;
        ps.solidMaterial = mat;
        ps.ghostMaterial = ghost;
        Rigidbody rb = go.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;

        go.tag = "Build";
        go.layer = IGNORERAYCAST;

        return go;
    }
}