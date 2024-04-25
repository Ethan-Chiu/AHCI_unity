using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


[Serializable]
public class SignalingMessage
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SignalingMessageType Type;

    public string Message;

    public string ConvertToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static SignalingMessage FromJSON(string messageString)
    {
        return JsonConvert.DeserializeObject<SignalingMessage>(messageString);
    }
}
