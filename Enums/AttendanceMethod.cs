using System.ComponentModel;

namespace ArgosApi.Enums;

public enum AttendanceMethod
{
    Unknown = 0,

    [Description("Finger Print")]
    FingerPrint = 1,

    [Description("Password")]
    Password = 3,
    
    [Description("Face")]
    Face = 15,
}
