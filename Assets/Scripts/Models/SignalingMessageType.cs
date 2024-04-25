using System.Runtime.Serialization;

public enum SignalingMessageType
{
    [EnumMember(Value = "offer")]
    OFFER,
    [EnumMember(Value = "answer")]
    ANSWER,
    [EnumMember(Value = "candidate")]
    CANDIDATE,
    [EnumMember(Value = "other")]
    OTHER
}