using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Converters;


[Serializable]
public class SignalingMessage
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SignalingMessageType Type;

    public string Message;

    public static SignalingMessage FromJson(string messageString)
    {
        return JsonConvert.DeserializeObject<SignalingMessage>(messageString);
    }
}
