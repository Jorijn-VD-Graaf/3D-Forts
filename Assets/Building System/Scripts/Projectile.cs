using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 150;
    public float health = 150;
    public float force = 100;

    public GameObject explosion;

    private void FixedUpdate() {
        if (GetComponent<Rigidbody>()) {
            transform.rotation = Quaternion.LookRotation(Vector3.Normalize(transform.GetComponent<Rigidbody>().velocity));
        }
        if (health <= 0) {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject && collision.gameObject.tag == "Build") {
            if (collision.gameObject.GetComponent<PlatformHealth>().Damage(damage)) {
                health -= damage;
                collision.gameObject.GetComponent<PlatformHealth>().Explode(collision.GetContact(0).point, force);
            } else {
                health = 0.0f;
            }
        } else {
            health = 0.0f;
        }
    }

    private void Explode () {
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
