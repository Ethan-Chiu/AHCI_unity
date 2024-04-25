using System.Net.Sockets;
using System.Net;
using UnityEngine;
using WebSocketSharp.Server;

public class SimpleDataChannelServer : MonoBehaviour
{
    private WebSocketServer wssv;
    private string serverIpv4Address;
    private int serverPort = 8080;

    // Start is called before the first frame update
    private void Awake()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                serverIpv4Address = ip.ToString();
                break;
            }
        }

        wssv = new WebSocketServer($"ws://{serverIpv4Address}:{serverPort}");
        Debug.Log(serverIpv4Address);

        wssv.AddWebSocketService<SimpleDataChannelService>($"/{nameof(SimpleDataChannelService)}");
        wssv.Start();
    }

    private void OnDestroy()
    {
        wssv.Stop();
    }
}
