using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

    private Dictionary<int, Color> originalColors;
    MeshRenderer[] meshRenderers;

    private void Start() {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originalColors = new Dictionary<int, Color>();
        int i = 0;
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            foreach (Material material in meshRenderer.materials) {
                originalColors.Add(i, material.color);
                i++;
            }
        }
    }
    public void ResetColor() {
        int i = 0;
        if (meshRenderers != null) {
            foreach (MeshRenderer meshRenderer in meshRenderers) {
                foreach (Material material in meshRenderer.materials) {
                    material.color = originalColors[i];
                    i++;
                }
            }
        }
    }
    public void ChangeColor(Color color) {
        if (meshRenderers != null) {
            foreach (MeshRenderer meshRenderer in meshRenderers) {
                foreach (Material material in meshRenderer.materials) {
                    material.color = color;
                }
            }
        }
    }
}
