namespace Jacana.Inpatient.Domain;

/// <summary>Category of a ward — mirrors Kenyan hospital ward conventions.</summary>
public enum WardType
{
    General,
    Maternity,
    Pediatric,
    Surgical,
    Icu,
    Isolation,
    Private,
    Recovery
}
