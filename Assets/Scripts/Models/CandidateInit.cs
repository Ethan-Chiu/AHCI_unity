using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

[Serializable]
public class CandidateInit : IJsonObject<CandidateInit>
{
    [JsonConverter(typeof(StringEnumConverter)), JsonProperty("type")]
    public SignalingMessageType Type = SignalingMessageType.CANDIDATE;

    [JsonProperty("candidate")]
    public string Candidate;

    [JsonProperty("id")]
    public string SdpMid;

    [JsonProperty("label")]
    public int SdpMLineIndex;


    public string ConvertToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static CandidateInit FromJSON(string jsonString)
    {
        return JsonConvert.DeserializeObject<CandidateInit>(jsonString);
    }

}
