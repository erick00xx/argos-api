using System.ComponentModel;

namespace ArgosApi.Enums;
public enum Manufacturer
{
    Unknown = 0,
    [Description("Workera")]
    Workera = 1,
    [Description("Zkteco")]
    Zkteco = 2
}