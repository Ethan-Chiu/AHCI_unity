using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectPoint : MonoBehaviour
{
    public OVRCameraRig ovrCameraRig;
    public GameObject planeObject; // The GameObject representing the plane

    public OVRSkeleton skeleton;
    public Vector3 positionOffset = new Vector3(0, 0.1f, 0.1f);

    public Material mat;

    public Vector2 projectedPos; // Projected
    public GameObject blackDot;

    void Start()
    {
        if (blackDot == null || planeObject == null)
        {
            Debug.LogError("Blackdot 或 Plane 对象未分配！");
            return;
        }
    }

    void Update()
    {
        // Get the normal of the plane (assuming the plane's normal is the Transform's up vector)
        Vector3 cameraPoint = ovrCameraRig.centerEyeAnchor.position;
        Transform planeTransform = planeObject.transform;
        Vector3 pointToProject = skeleton.transform.position + positionOffset;

        projectedPos = ProjPointToPlane(cameraPoint, pointToProject, planeTransform);

        if (mat != null)
        {
            mat.SetVector("_Offset", projectedPos);
        }
        else
        {
            Debug.Log("no mat no mat");
        }

        // if ( !(float.IsNaN(planePoint.x) || float.IsNaN(planePoint.y) || float.IsNaN(planePoint.z)) )
        //     blackDot.transform.position = planePoint;
    }

    Vector2 ProjPointToPlane(Vector3 camPos, Vector3 pointPos, Transform planeTransform)
    {
        Vector3 planeNormal = planeTransform.forward;
        Vector3 planePos = planeTransform.position;
        planeNormal.Normalize();

        Vector3 g = pointPos - camPos;
        float t1 = Vector3.Dot((planePos - camPos), planeNormal);
        float t2 = Vector3.Dot(g, planeNormal);
        float t = Mathf.Abs(t1) / Mathf.Abs(t2);
        Vector3 planePoint = camPos + g * t;

        Vector3 dis = planePoint - planePos;

        if (Vector3.Dot(dis, planeNormal) > 0.0001)
            Debug.Log("Error solution");

        Vector3 scale = planeTransform.localScale;
        float upDis = Vector3.Dot(dis, planeTransform.up) / scale.x;
        float rightDis = Vector3.Dot(dis, planeTransform.right) / scale.y;

        return new Vector2(rightDis, upDis);
    }

}