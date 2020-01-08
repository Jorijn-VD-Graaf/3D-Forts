using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public int team;
    private bool IsDevice;

    
    private void Start()
    {
        CheckIfIsDevice();
    }

    private void CheckIfIsDevice ()
    {
        if (GetComponent<PlaceablesScript>())
        {
            IsDevice = true;
        }
        else if (GetComponent<PlatformStrut>())
        {
            IsDevice = false;
        }
        else
        {
            Debug.LogError("Error: Not Device or material - Team Script on gameobject: " + gameObject);
        }
    }

}
