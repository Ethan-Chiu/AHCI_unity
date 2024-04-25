using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]

public class SessionDescription : IJsonObject<SessionDescription>
{
    [JsonProperty("type")]
    public string SessionType;

    [JsonProperty("sdp")]
    public string Sdp;

    // Start is called before the first frame update
    public string ConvertToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public static SessionDescription FromJSON(string jsonString)
    {
        return JsonUtility.FromJson<SessionDescription>(jsonString);
    }
}
