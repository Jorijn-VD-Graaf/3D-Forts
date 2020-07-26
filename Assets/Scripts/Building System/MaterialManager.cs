using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {
    Material[] materials;

    public void UpdateMats(Texture texture) {
        materials = GetComponent<MeshRenderer>().materials;
        for (int i = 0; i < materials.Length; i++) {
            materials[i].mainTexture = texture;
        }
    }
    public void UpdateColor(Color color) {
        materials = GetComponent<MeshRenderer>().materials;
        for (int i = 0; i < materials.Length; i++) {
            materials[i].color = color;
        }
    }
}
