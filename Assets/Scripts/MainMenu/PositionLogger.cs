using UnityEngine;

public class PositionLogger : MonoBehaviour
{
    public float globalX;
    public float globaY;
    public float globalZ;
    public float localX;
    public float localY;
    public float localZ;
    public float sizeX;
    public float sizeY;

    public RectTransform rect;
    public void Start()
    {
        rect = transform.gameObject.GetComponent<RectTransform>();
    }
    public void Update()
    {
        globalX = gameObject.transform.position.x;
        globaY = gameObject.transform.position.y;
        globalZ = gameObject.transform.position.z;
        localX = gameObject.transform.localPosition.x;
        localY = gameObject.transform.localPosition.y;
        localZ = gameObject.transform.localPosition.z;
        sizeX = rect.sizeDelta.x;
        sizeY = rect.sizeDelta.y;
    }
}
