namespace SistemaNotasColegio.DTOs.Boletines;

public record BoletinMateriaDto(
    int MateriaId,
    string MateriaCodigo,
    string MateriaNombre,
    decimal? NotaPrimerTrimestre,
    decimal? NotaSegundoTrimestre,
    decimal? NotaTercerTrimestre,
    int? PromedioAnual,
    string Estado);
