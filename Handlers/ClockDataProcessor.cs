using ArgosApi.Dtos;
using ArgosApi.Services;
using System.Globalization;

namespace ArgosApi.Handlers;

public class ClockDataProcessor
{
    private readonly IAttendanceService _attendanceService;

    public ClockDataProcessor(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // Este es el método al que el Controlador le pasará la pelota
    public async Task<bool> ProcessClockDataAsync(string sn, string table, string body)
    {
        switch (table.ToUpper())
        {
            case "ATTLOG":
                List<AttendanceLogDto> attendances = AttendanceParser(sn, table, body);

                if (attendances.Any())
                    return await _attendanceService.SaveMultipleAttendancesAsync(attendances);
                break;

            case "OPERLOG":
                List<UsuarioDto> usuarios = UsuarioParser(body);

                if (usuarios.Any())
                    // await _usuarioService.ActualizarUsuariosDelReloj(usuarios, sn);
                    Console.WriteLine($"Procesar {usuarios.Count} usuarios para el reloj {sn}");
                return true; // Retorna true para simular que se procesó correctamente, aunque la lógica real aún no esté implementada
                // break;

            case "BIODATA":
                // Lógica de huellas...
                return true; // Retorna true para simular que se procesó correctamente, aunque la lógica real aún no esté implementada
                // break;

            default:
                Console.WriteLine($"Lógica no implementada para la tabla: {table} del reloj {sn} con el body: {body}");
                return true; // Retorna true para evitar reintentos en tablas no implementadas, pero loguea la situación para futuras implementaciones

        }
        return false;
    }


    private List<AttendanceLogDto> AttendanceParser(string sn, string table, string rawBody)
    {
        var lista = new List<AttendanceLogDto>();

        var lineas = rawBody.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var linea in lineas)
        {
            var columnas = linea.Split('\t');

            if (columnas.Length >= 4)
            {
                try
                {
                    string pinRaw = columnas[0].Trim();
                    string dateTimeRaw = columnas[1].Trim();
                    string punchTypeRaw = columnas[2].Trim();
                    string methodRaw = columnas[3].Trim();

                    if (DateTime.TryParseExact(dateTimeRaw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime punchDateTime))
                    {
                        // int.TryParse(punchTypeRaw, out int punchTypeInt);
                        // int.TryParse(methodRaw, out int methodInt);

                        var dto = new AttendanceLogDto
                        {
                            Sn = sn,
                            Table = table.ToUpper(),
                            Pin = pinRaw,
                            PunchDateTime = punchDateTime,
                            PunchType = punchTypeRaw,
                            Method = methodRaw
                        };

                        lista.Add(dto);
                        Console.WriteLine($"[INFO] Línea ATTLOG parseada correctamente: PIN={dto.Pin}, DateTime={dto.PunchDateTime}, PunchType={dto.PunchType}, Method={dto.Method}");
                    }
                    else
                    {
                        Console.WriteLine($"[ADVERTENCIA] No se pudo parsear la fecha: {dateTimeRaw} para el PIN: {pinRaw}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Falló el parseo de la línea ATTLOG: {linea}. Detalle: {ex.Message}");
                }
            }
        }

        return lista;
    }

    private List<UsuarioDto> UsuarioParser(string rawBody)
    {
        var list = new List<UsuarioDto>();
        // Tu lógica de buscar "USER PIN=" que vimos antes...
        return list;
    }
}