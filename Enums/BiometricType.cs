using System.ComponentModel;

namespace ArgosApi.Enums;
public enum BiometricType
{
    Unknown = 0,
    [Description("Finger Print")]
    FingerPrint = 1,
    [Description("Face")]
    Face = 15,
}