namespace Jacana.Clinical.Domain;

/// <summary>Category of a patient flag (a sticky alert on the record).</summary>
public enum PatientFlagType
{
    /// <summary>Known allergy / adverse reaction alert.</summary>
    Allergy,
    /// <summary>Safety or clinical warning (fall risk, NPO, isolation…).</summary>
    Warning,
    /// <summary>Informational note (language, special needs…).</summary>
    Info,
    /// <summary>Medical condition note (diabetic, asthmatic…).</summary>
    Medical
}
