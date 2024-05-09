using Oculus.Interaction.Input;
using System;
using UnityEngine;
using WebSocketSharp;
using System.Net.Sockets;
using System.Net;

public class SendHandData : MonoBehaviour
{
    [SerializeField]
    private Hand leftHand;

    [SerializeField]
    private Hand rightHand;

    [SerializeField]
    private GameObject head;


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
    }

    void Update()
    {
        var handData = "";
        for (HandJointId jointId = HandJointId.HandStart; jointId < HandJointId.HandEnd; jointId++)
        {
            leftHand.GetJointPose(jointId, out currentLeftPose);
            rightHand.GetJointPose(jointId, out currentRightPose);
            handData += (jointId.ToString() + "; "
                + "left:" + currentLeftPose.position + currentLeftPose.rotation + "; "
                + "right:" + currentRightPose.position + currentRightPose.rotation + ";");
            handData += "\n";
        }
        var headData = "head_pos:" + head.transform.position + "; "
                + "head_rot:" + head.transform.rotation + ";";
        handData += headData + "\n\n";

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
}