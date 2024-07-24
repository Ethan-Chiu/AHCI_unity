using Oculus.Interaction.Input;
using System;
using UnityEngine;
using WebSocketSharp;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using Unity.Mathematics;

public class SendHandData : MonoBehaviour
{
    [SerializeField]
    private Hand leftHand;

    [SerializeField]
    private Hand rightHand;

    [SerializeField]
    private GameObject head;

    public OVRSkeleton skeleton;
    public List<Gesture> gestures;

    public float threshold = 0.04f;
    public Material mat;

    public GameObject projectHnadObject;
    private ProjectPoint projectHandScript;

    // private List<Vector3> leftJointPos;
    private List<Vector3> rightJointPos = new List<Vector3>();

    private Pose currentLeftPose;
    private Pose currentRightPose;

    private WebSocket ws;
    private string clientId;

    private void Start()
    {
        string serverIpv4Address = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                serverIpv4Address = ip.ToString();
                break;
            }
        }
        
        if (serverIpv4Address == "")
        {
            Debug.Log("No intern host address found");
        }
        Debug.Log(serverIpv4Address);

        InitClient(serverIpv4Address, 8080);
        ws.Send("Time!" + DateTime.Now.ToString());

        projectHandScript = projectHnadObject.GetComponent<ProjectPoint>();
    }

    void Update()
    {
        var handData = "";
        rightJointPos.Clear();
        for (HandJointId jointId = HandJointId.HandStart; jointId < HandJointId.HandEnd; jointId++)
        {
            leftHand.GetJointPose(jointId, out currentLeftPose);
            rightHand.GetJointPose(jointId, out currentRightPose);
            handData += (jointId.ToString() + "; "
                + "left:" + currentLeftPose.position + currentLeftPose.rotation + "; "
                + "right:" + currentRightPose.position + currentRightPose.rotation + ";");
            handData += "\n";
            // leftJointPos[((int)jointId)] = currentLeftPose.position;
            rightJointPos.Add(currentRightPose.position);
        }
        var headData = "head_pos:" + head.transform.position + "; "
                + "head_rot:" + head.transform.rotation + ";";
        handData += headData + "\n\n";

        Debug.Log(handData);

        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());

        Debug.Log(hasRecognized);

        if ( hasRecognized )
        {
            Debug.Log(currentGesture.name);
            handData += "torch";
            handData += (projectHandScript.projectedPos[0].ToString() + "," + 
                projectHandScript.projectedPos[1].ToString());
        }

        if (mat != null)
        {
            mat.SetFloat("_TorchMode", hasRecognized ? 1.0f : 0.0f);
        }
        else
        {
            Debug.Log("no mat no mat");
        }

        ws.Send(handData);
    }

    public void InitClient(string serverIp, int serverPort)
    {
        int port = serverPort == 0 ? 8080 : serverPort;
        clientId = gameObject.name;
        Debug.Log(clientId);

        string endpoint = $"ws://{serverIp}:{port}/posedata";
        Debug.Log("Reciever endpoint " + endpoint);
        ws = new WebSocket(endpoint);

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Reciever got: " + e.Data);
        };
        ws.Connect();
    }

    Gesture Recognize()
    {
        Gesture currentgesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < rightJointPos.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(rightJointPos[i]);
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