using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingScript : MonoBehaviour {
    public GameObject bomb;
    public AudioClip fireSound;
    AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(2)) {
            if (!audioSource.isPlaying) {
                GameObject gArrow = Instantiate(bomb, transform.position, transform.rotation);
                gArrow.GetComponent<Rigidbody>().AddForce(transform.forward*30, ForceMode.VelocityChange);
                audioSource.clip = fireSound;
                audioSource.Play();
            }
        }
    }
}