using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    float fadeTime = 5.0f;
    float timer = 0.0f;
    bool fade = false;

    private void FixedUpdate() {
        if (fade) {
            timer += Time.deltaTime;
            if (timer >= fadeTime) {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        fade = true;
    }
}
