using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}

public class GestureDetector : MonoBehaviour
{
    public Vector3 positionOffset = new Vector3(0, 0.1f, 0.1f);
    public float threshold = 0.1f;
    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    public bool debugMode = true;

    private List<OVRBone> fingerBones;
    private Gesture previousGesture;
    private bool ready = false;

    public Material mat;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (skeleton.Bones.Count == 0)
        {
            yield return null;
        }
        fingerBones = new List<OVRBone>(skeleton.Bones);
        previousGesture  =  new Gesture();
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {   
        if (!ready)
        {
            return;
        }
        if(debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }
        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());
        if (mat != null)
        {
            mat.SetFloat("_TorchMode", hasRecognized ? 1.0f : 0.0f);
        }
        else
        {
            Debug.Log("no mat no mat");
        }

        if(currentGesture.name == "OK")
        {
            Debug.Log("New Gesture Found: " + currentGesture.name + " " + skeleton.transform.position);
            transform.position = skeleton.transform.position + positionOffset;
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
        }
    }

    void Save()
    {
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach (var bone in fingerBones)
        {
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));

        }
        g.fingerDatas = data;
        gestures.Add(g);
    }

    Gesture Recognize()
    {
        Gesture currentgesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentgesture = gesture;
            }
        }
        return currentgesture;
    }
}
