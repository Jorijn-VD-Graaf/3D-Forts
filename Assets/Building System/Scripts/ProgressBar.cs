using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image bar;
    public Image icon;
    public GameObject attachedObject;
    Camera myCamera;

    static int BUILD = 0;
    static int TRASH = 1;
    static int REPAIR = 2;
    static int RELOAD = 3;

    public float percent = 0.0f;
    float startTime;

    public Sprite buildsprite;
    public Sprite buildIcon;
    public Sprite trashsprite;
    public Sprite trashIcon;
    public Sprite repairsprite;
    public Sprite repairIcon;

    int currentType = BUILD;

    void Awake() {
        myCamera = Camera.main;
        startTime = Time.time;
    }

    void FixedUpdate()
    {
        if (attachedObject) {
            if (Vector3.Dot(myCamera.transform.forward, attachedObject.transform.position - myCamera.transform.position) > 0) {
                transform.position = myCamera.WorldToScreenPoint(attachedObject.transform.position);
            }
            if (ProgressChange(percent)) {
                Destroy(gameObject);
            }
            float distance = (5.5f - ((Vector3.Distance(myCamera.transform.position, attachedObject.transform.position) < 2.5f) ? (Vector3.Distance(myCamera.transform.position, attachedObject.transform.position)) : 2.5f))/10;
            Vector3 scale = new Vector3(distance, distance, distance);
            transform.localScale = scale;
        } else {
            Destroy(gameObject);
        }
    }

    bool ProgressChange(float percent) {
        if (currentType == BUILD || currentType == RELOAD || currentType == REPAIR) {
            bar.fillAmount = percent;
            return percent >= 1;
        } else if (currentType == TRASH) {
            bar.fillAmount =  1 - percent;
            return percent >= 1;
        }
        return false;
    }

    public void ChangeIcon(int type) {
        if (type == BUILD) {
            bar.sprite = buildsprite;
            icon.sprite = buildIcon;
        }
        if (type == TRASH) {
            bar.sprite = trashsprite;
            icon.sprite = trashIcon;
        }
        if (type == REPAIR) {
            bar.sprite = repairsprite;
            icon.sprite = repairIcon;
        }
    }
}
