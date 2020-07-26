using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{

    public List<GameObject> Projectiles;

    public GameObject GetProjectileByName(string NameToFind) { 
        foreach (GameObject Projectile in Projectiles) {
            if (NameToFind == Projectile.name) {
                return Projectile;
            }
        }
        return null;
    }
}
