namespace SistemaNotasColegio.Services.Common;

public static class ReglasAcademicas
{
    public const decimal NotaMinima = 0m;

    public const decimal NotaMaxima = 100m;

    public const int NotaMinimaAprobacion = 51;

    public const int TotalTrimestres = 3;

    public const string EstadoAprobado = "APROBADO";

    public const string EstadoReprobado = "REPROBADO";

    public const string EstadoIncompleto = "INCOMPLETO";

    /// <summary>
    /// Calcula el promedio anual de las tres notas trimestrales.
    /// Devuelve null si falta cualquiera de las tres notas.
    /// El redondeo se hace al entero mas cercano con MidpointRounding.AwayFromZero:
    /// un valor terminado en .5 se aleja de cero (por ejemplo, 50.5 se redondea a 51).
    /// </summary>
    public static int? CalcularPromedioAnual(decimal? primero, decimal? segundo, decimal? tercero)
    {
        if (primero is null || segundo is null || tercero is null)
        {
            return null;
        }

        var promedio = (primero.Value + segundo.Value + tercero.Value) / TotalTrimestres;

        return (int)Math.Round(promedio, 0, MidpointRounding.AwayFromZero);
    }

    public static string CalcularEstado(int? promedioAnual)
    {
        if (promedioAnual is null)
        {
            return EstadoIncompleto;
        }

        return promedioAnual.Value >= NotaMinimaAprobacion
            ? EstadoAprobado
            : EstadoReprobado;
    }
}
