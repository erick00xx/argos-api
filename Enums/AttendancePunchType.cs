using System.ComponentModel;

namespace ArgosApi.Enums;

public enum AttendancePunchType
{
    [Description("Check In")]
    CheckIn = 0,
    [Description("Check Out")]
    CheckOut = 1,
    [Description("Break Start")]
    BreakStart = 2,
    [Description("Break End")]
    BreakEnd = 3
}
