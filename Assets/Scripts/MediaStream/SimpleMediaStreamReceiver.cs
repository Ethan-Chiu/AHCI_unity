using System.Collections;
using Unity.WebRTC;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class SimpleMediaStreamReceiver : MonoBehaviour
{
    [SerializeField] private RawImage receiveImage;

    private RTCPeerConnection connection;

    private WebSocket ws;
    private string clientId;

    private bool hasReceivedOffer = false;
    private SessionDescription receivedOfferSessionDescTemp;

    private void Start()
    {
        InitClient("192.168.137.1", 8080);
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

        ws.OnMessage += (sender, e) =>
        {
            var signalingMessage = new SignalingMessage(e.Data);

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
            ws.Send("CANDIDATE!" + candidateInit.ConvertToJSON());
        };
        connection.OnIceConnectionChange = state =>
        {
            Debug.Log(state);
        };

        connection.OnTrack = e =>
        {
            if (e.Track is VideoStreamTrack video)
            {
                video.OnVideoReceived += tex =>
                {
                    receiveImage.texture = tex;
                };
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
            SessionType = answerDesc.type.ToString(),
            Sdp = answerDesc.sdp
        };
        ws.Send("OFFER!" + answerSessionDesc.ConvertToJSON());
    }
}