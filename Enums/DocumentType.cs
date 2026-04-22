using System.ComponentModel;

namespace ArgosApi.Enums;
public enum DocumentType
{
    Unknown = 0,
    [Description("Documento Nacional de Identidad")]
    DNI = 1,
    [Description("Passaporte")]
    Passport = 2,
}