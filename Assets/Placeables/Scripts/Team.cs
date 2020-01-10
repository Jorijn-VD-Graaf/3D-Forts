using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public int team;
    private bool IsDevice;

    
    void Start()
    {
        CheckIfIsDevice();
        if (IsDevice)
        {
            SetTeamToMaterial();
        }
    }

    // Check if it is a device, material or neather.
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

    private void SetTeamToMaterial()
    {
        var JointFixed = gameObject.GetComponent<FixedJoint>();
        if (JointFixed != null)
        {
            var RigBodyOther = JointFixed.connectedBody;
            var Material = RigBodyOther.gameObject;
            if (Material.GetComponent<Team>() != null)
            {
                team = Material.GetComponent<Team>().team;
            }
        }
    }

}
