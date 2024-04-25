using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;

[Serializable]

public class SessionDescription : IJsonObject<SessionDescription>
{
    [JsonConverter(typeof(StringEnumConverter)), JsonProperty("type")]
    public SignalingMessageType Type;

    [JsonProperty("sdp")]
    public string Sdp;

    // Start is called before the first frame update
    public string ConvertToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static SessionDescription FromJSON(string jsonString)
    {
        return JsonConvert.DeserializeObject<SessionDescription>(jsonString);
    }
}
