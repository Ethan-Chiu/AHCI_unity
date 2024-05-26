using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlackDot : MonoBehaviour
{
    public OVRCameraRig ovrCameraRig;
    public GameObject trackObject;
    public GameObject planeObject; // The GameObject representing the plane
    public GameObject blackDot;

    public Material mat;

    public Vector2 pos; // The point you want to project

    void Start()
    {
        if (blackDot == null || planeObject == null)
        {
            Debug.LogError("Blackdot 或 Plane 对象未分配！");
            return;
        }
    }


    public void UpdateBlackDotPosition(float x, float y)
    {
        float localX = Mathf.Clamp(x, -0.5f, 0.5f);
        float localY = Mathf.Clamp(y, -0.5f, 0.5f);  
        Vector3 localPosition = new Vector3(localX, localY, 0);
        Vector3 worldPosition = planeObject.transform.TransformPoint(localPosition);

        blackDot.transform.position = worldPosition;
    }

    void Update()
    {
        // Get the normal of the plane (assuming the plane's normal is the Transform's up vector)
        Transform centerEyeAnchor = ovrCameraRig.centerEyeAnchor;

        Vector3 planeNormal = planeObject.transform.forward;
        planeNormal.Normalize();
        Vector3 pointToProject = trackObject.transform.position;

        Vector3 planePos = planeObject.transform.position;
        Vector3 cameraPoint = centerEyeAnchor.position;

        Vector3 g = pointToProject - cameraPoint;
        float t1 = Vector3.Dot((planePos - cameraPoint), planeNormal);
        float t2 = Vector3.Dot(g, planeNormal);
        float t = Mathf.Abs(t1) / Mathf.Abs(t2);
        Vector3 planePoint = cameraPoint + g * t;

        Vector3 dis = planePoint - planePos;

        if (Vector3.Dot(dis, planeNormal) > 0.0001)
            Debug.Log("Error solution");

        Vector3 scale = planeObject.transform.localScale;
        float upDis = Vector3.Dot(dis, planeObject.transform.up) / scale.x;
        float rightDis = Vector3.Dot(dis, planeObject.transform.right) / scale.y;

        Debug.Log("upDis" + upDis);
        Debug.Log("rightDis" + rightDis);

        if (mat != null)
        {
            mat.SetVector("_Offset", new Vector2(rightDis, upDis));
        }
        else
        {
            Debug.Log("no mat no mat");
        }

        blackDot.transform.position = planePoint;

        /* float xDis = (dis.x) / scale.x;
        float yDis = (dis.y) / scale.y;
      
        UpdateBlackDotPosition(xDis, yDis);*/
        pos = new Vector2(upDis, rightDis);
    }
}