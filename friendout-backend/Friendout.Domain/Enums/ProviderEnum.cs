using System.Runtime.Serialization;

namespace Friendout.Domain.Enums;

public enum ProviderEnum
{
    [EnumMember(Value = "DISCORD")]
    Discord,
    
    [EnumMember(Value = "GOOGLE")]
    Google
}