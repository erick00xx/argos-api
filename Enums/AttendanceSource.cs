using System.ComponentModel;

namespace ArgosApi.Enums;

public enum AttendanceSource
{
    Unknown = 0,
    [Description("Manual")]
    Manual = 1,
    [Description("Device")]
    Device = 2,
    [Description("Web")]
    Web = 3,
    [Description("Mobile")]
    Mobile = 4
}
