using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class WarningBoxScript : MonoBehaviour
{
    public float TimeShowing = 0.5f;
    public Text TextBox;

    private void Start()
    {
        gameObject.GetComponent<Image>().enabled = false;
        TextBox.enabled = false;
    }
    public void SetWarning(string WarningText) {
        gameObject.GetComponent<Image>().enabled = true;
        TextBox.enabled = true;
        TextBox.text = WarningText;
        CancelInvoke("DeactivateActivateSelf");
        Invoke("DeactivateActivateSelf", TimeShowing);
    }
    private void DeactivateActivateSelf() {
        gameObject.GetComponent<Image>().enabled = false;
        TextBox.enabled = false;
    }

}
