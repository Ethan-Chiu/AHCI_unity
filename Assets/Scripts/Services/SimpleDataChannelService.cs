using System.Linq;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class SimpleDataChannelService : WebSocketBehavior
{
    protected override void OnOpen()
    {
        var session = Sessions.Sessions.First(s => s.ID == ID);
        Debug.Log("SERVER SimpleDataChannelService started! " +session.ID);
    }
    // Start is called before the first frame update
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(ID + " - DataChannel SERVER got message " + e.Data);

        foreach (var id in Sessions.ActiveIDs)
        {
            if (id != ID)
            {
                Debug.Log("Send to " + id);
                Sessions.SendTo(e.Data, id);
            }
        }
    }
}
