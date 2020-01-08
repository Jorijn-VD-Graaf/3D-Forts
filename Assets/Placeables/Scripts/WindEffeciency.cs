using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindEffeciency : MonoBehaviour {
    private List<Collider> TriggerList = new List<Collider>();


    private readonly float ApproxementTriggerLength = 1;

    private void CollidersInsideBoxToTriggerList() {
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, new Vector3(ApproxementTriggerLength, 1, ApproxementTriggerLength), Quaternion.identity);

        foreach(Collider TheCollider in hitColliders) { 
            if (TheCollider.gameObject.layer != LayerMask.NameToLayer("terrain") && TheCollider.gameObject.layer != LayerMask.NameToLayer("PlacedPlaceables"))
            {
                TriggerList.Clear();
                TriggerList.Add(TheCollider);
                //Debug.Log(TheCollider.gameObject);
            }
        }
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode

    //        //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
    //        Gizmos.DrawWireCube(transform.position, new Vector3(ApproxementTriggerLength * 2, 1, ApproxementTriggerLength * 2));
        
    //}

    public float CheckWindEffiecency() {
        CollidersInsideBoxToTriggerList();
        if (TriggerList.Count != 0)
        {


            List<int> RightSide = new List<int>();
            List<int> LeftSide = new List<int>();
            List<int> BackSide = new List<int>();
            List<int> FrontSide = new List<int>();

            for (int i = 0; i < TriggerList.Count; i++)
            {
                Vector3 closestPoint = TriggerList[i].ClosestPoint(transform.position);
                int side = GetSide(closestPoint);
                if (side == 0)
                {
                    RightSide.Add(i);
                }
                else if (side == 1)
                {
                    LeftSide.Add(i);
                }
                else if (side == 2)
                {
                    FrontSide.Add(i);
                }
                else if (side == 3)
                {
                    BackSide.Add(i);
                }

            }

            var ClosestBackSide = FindClosestUsingIndex(BackSide);
            var ClosestFrontSide = FindClosestUsingIndex(FrontSide);
            var ClosestLeftSide = FindClosestUsingIndex(LeftSide);
            var ClosestRightSide = FindClosestUsingIndex(RightSide);
            var BackSideEffiecency = 1f;
            var FrontSideEffiecency = 1f;
            var LeftSideEffiecency = 1f;
            var RightSideEffiecency = 1f;
            if (ClosestBackSide != -1) {
                //DrawLine(TriggerList[ClosestBackSide].ClosestPoint(transform.position), transform.position, Color.green, 1); //for debugging

                BackSideEffiecency = Vector3.Distance(TriggerList[ClosestBackSide].ClosestPoint(transform.position), transform.position) / ApproxementTriggerLength;
            } 
            if (ClosestFrontSide != -1) {
                //DrawLine(TriggerList[ClosestFrontSide].ClosestPoint(transform.position), transform.position, Color.green, 1);
                FrontSideEffiecency = Vector3.Distance(TriggerList[ClosestFrontSide].ClosestPoint(transform.position), transform.position) / ApproxementTriggerLength;
            }
            if (ClosestLeftSide != -1)
            {
                //DrawLine(TriggerList[ClosestLeftSide].ClosestPoint(transform.position), transform.position, Color.green, 1);
                LeftSideEffiecency = Vector3.Distance(TriggerList[ClosestLeftSide].ClosestPoint(transform.position), transform.position) / ApproxementTriggerLength;
            }
            if (ClosestRightSide != -1)
            {
                //DrawLine(TriggerList[ClosestRightSide].ClosestPoint(transform.position), transform.position, Color.green, 1);
                RightSideEffiecency = Vector3.Distance(TriggerList[ClosestRightSide].ClosestPoint(transform.position), transform.position) / ApproxementTriggerLength;
            }


            if (RightSideEffiecency > 1)
            {
                RightSideEffiecency = 1;
            }
            if (LeftSideEffiecency > 1)
            {
                LeftSideEffiecency = 1;
            }
            if (FrontSideEffiecency > 1)
            {
                FrontSideEffiecency = 1;
            }
            if (BackSideEffiecency > 1)
            {
                BackSideEffiecency = 1;
            }
            //Debug.Log(RightSideEffiecency + " " + LeftSideEffiecency + " " + BackSideEffiecency + " " + FrontSideEffiecency);
            var CurrentEffeciency = (RightSideEffiecency + LeftSideEffiecency + BackSideEffiecency + FrontSideEffiecency) / 4;
            //Debug.Log(CurrentEffeciency);
            return CurrentEffeciency;
        }
        return 1;

    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.SetColors(color, color);
        lr.SetWidth(0.1f, 0.1f);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(myLine, duration);
    }


    private int FindClosestUsingIndex(List<int> IndexList) {
        if (IndexList.Count != 0) {


            List<float> Distances = new List<float>();

            for (int i = 0; i < IndexList.Count; i++)
            {

                int CurrentIndex = IndexList[i];

                Distances.Add(Vector3.Distance(TriggerList[CurrentIndex].ClosestPoint(transform.position), transform.position));



            }

            float value = float.PositiveInfinity;
            int index = -1;

            for (int i = 0; i < Distances.Count; i++)
            {

                if (Distances[i] < value)
                {

                    index = i;



                    value = Distances[i];
                }

            }

            return IndexList[index];
        }
        return -1;

    }



    private int GetSide(Vector3 center) {
        var Distances = new float[4];
        var Sides = new Vector3[4];
        Vector3 colliderVector3right = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
        Vector3 colliderVector3left = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z );
        Vector3 colliderVector3front = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
        Vector3 colliderVector3back = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
       

        Distances[0] = Vector3.Distance(center, colliderVector3right);
        Distances[1] = Vector3.Distance(center, colliderVector3left);
        Distances[2] = Vector3.Distance(center, colliderVector3front);
        Distances[3] = Vector3.Distance(center, colliderVector3back);
        Sides[0] = colliderVector3right;
        Sides[1] = colliderVector3left;
        Sides[2] = colliderVector3front;
        Sides[3] = colliderVector3back;
        float value = float.PositiveInfinity;
        int index = -1;

        for (int i = 0; i < Distances.Length; i++)
        {

            if (Distances[i] < value)
            {

                index = i;



                value = Distances[i];
            }

        }

        return index;
    }
}
