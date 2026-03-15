using System.Runtime.Serialization;


namespace Friendout.Domain.Enums
{
    public enum UserRole
    {
        [EnumMember(Value = "USER")]
        User,
        
        [EnumMember(Value = "ADMIN")]
        Admin
    }
}
