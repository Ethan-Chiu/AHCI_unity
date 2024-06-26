using System.Collections;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System.Net.Sockets;
using System.Net;

public class SimpleMediaStreamReceiver : MonoBehaviour
{
    private RTCPeerConnection connection;

    private WebSocket ws;
    private string clientId;

    private bool hasReceivedOffer = false;
    private SessionDescription receivedOfferSessionDescTemp;

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
    }

    private void Update()
    {
        if (hasReceivedOffer)
        {
            hasReceivedOffer = !hasReceivedOffer;
            StartCoroutine(CreateAnswer());
        }
    }

    private void OnDestroy()
    {
        connection.Close();
        ws.Close();
    }

    public void InitClient(string serverIp, int serverPort)
    {
        int port = serverPort == 0 ? 8080 : serverPort;
        clientId = gameObject.name;

        string endpoint = $"ws://{serverIp}:{port}/video";
        Debug.Log("Reciever endpoint " + endpoint);
        ws = new WebSocket(endpoint);

        bool firstTrack = true;

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Reciever got: " + e.Data);
            var signalingMessage = SignalingMessage.FromJSON(e.Data);

            Debug.Log(signalingMessage.Type);
            Debug.Log(signalingMessage.Message);

            switch (signalingMessage.Type)
            {
                case SignalingMessageType.OFFER:
                    Debug.Log($"{clientId} - Got OFFER from Maximus: {signalingMessage.Message}");
                    receivedOfferSessionDescTemp = SessionDescription.FromJSON(signalingMessage.Message);
                    hasReceivedOffer = true;
                    break;
                case SignalingMessageType.CANDIDATE:
                    Debug.Log($"{clientId} - Got CANDIDATE from Maximus: {signalingMessage.Message}");

                    var candidateInit = CandidateInit.FromJSON(signalingMessage.Message);
                    RTCIceCandidateInit init = new RTCIceCandidateInit();
                    init.sdpMid = candidateInit.SdpMid;
                    init.sdpMLineIndex = candidateInit.SdpMLineIndex;
                    init.candidate = candidateInit.Candidate;
                    RTCIceCandidate candidate = new RTCIceCandidate(init);

                    connection.AddIceCandidate(candidate);
                    break;
                default:
                    Debug.Log(clientId + " - Maximus says: " + e.Data);
                    break;
            }
        };
        ws.Connect();

        connection = new RTCPeerConnection();

        connection.OnIceCandidate = candidate =>
        {
            var candidateInit = new CandidateInit()
            {
                SdpMid = candidate.SdpMid,
                SdpMLineIndex = candidate.SdpMLineIndex ?? 0,
                Candidate = candidate.Candidate
            };

            var msg = new SignalingMessage();
            msg.Type = SignalingMessageType.CANDIDATE;
            msg.Message = candidateInit.ConvertToJSON();

            ws.Send(msg.ConvertToJSON());
        };

        connection.OnIceConnectionChange = state =>
        {
            Debug.Log(state);
        };

        connection.OnTrack = e =>
        {
            
            if (e.Track is VideoStreamTrack video)
            {
                Debug.Log("video" + video.ToString() + " id " + video.Id);
                if (firstTrack)
                {
                    video.OnVideoReceived += tex =>
                    {
                        GetComponent<MeshRenderer>().material.SetTexture("_CamTexture", tex);
                    };
                    firstTrack = false;
                }
                else
                {
                    video.OnVideoReceived += tex =>
                    {
                        GetComponent<MeshRenderer>().material.SetTexture("_MaskTexture", tex);
                    };
                }
            }
        };

        StartCoroutine(WebRTC.Update());
    }

    private IEnumerator CreateAnswer()
    {
        RTCSessionDescription offerSessionDesc = new RTCSessionDescription();
        offerSessionDesc.type = RTCSdpType.Offer;
        offerSessionDesc.sdp = receivedOfferSessionDescTemp.Sdp;

        var remoteDescOp = connection.SetRemoteDescription(ref offerSessionDesc);
        yield return remoteDescOp;

        var answer = connection.CreateAnswer();
        yield return answer;

        var answerDesc = answer.Desc;
        var localDescOp = connection.SetLocalDescription(ref answerDesc);
        yield return localDescOp;

        var answerSessionDesc = new SessionDescription()
        {
            Type = SignalingMessageType.ANSWER,
            Sdp = answerDesc.sdp
        };

        var msg = new SignalingMessage();
        msg.Type = SignalingMessageType.ANSWER;
        msg.Message = answerSessionDesc.ConvertToJSON();
        ws.Send(msg.ConvertToJSON());
    }
}