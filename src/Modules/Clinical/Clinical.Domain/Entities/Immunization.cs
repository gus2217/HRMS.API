using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A vaccination record for a patient. Mirrors the OpenMRS "Immunizations"
/// patient-chart widget — captures the vaccine, dose number, administration and
/// next-due dates, lot number and injection site so immunisation schedules
/// (especially MCH / child-welfare clinics) can be followed.
/// </summary>
public sealed class Immunization : AggregateRoot<Guid>
{
    private Immunization() { } // EF

    private Immunization(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        string vaccineName,
        int doseNumber,
        DateTime administeredDate,
        DateTime? nextDueDate,
        string? lotNumber,
        string? site,
        string? notes,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        VaccineName = vaccineName;
        DoseNumber = doseNumber;
        AdministeredDate = administeredDate;
        NextDueDate = nextDueDate;
        LotNumber = lotNumber;
        Site = site;
        Notes = notes;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string VaccineName { get; private set; } = string.Empty;
    public int DoseNumber { get; private set; }
    public DateTime AdministeredDate { get; private set; }
    public DateTime? NextDueDate { get; private set; }
    public string? LotNumber { get; private set; }
    public string? Site { get; private set; }
    public string? Notes { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    public static Result<Immunization> Record(
        FacilityId facilityId,
        Guid patientId,
        string vaccineName,
        int doseNumber,
        DateTime administeredDate,
        DateTime? nextDueDate,
        string? lotNumber,
        string? site,
        string? notes,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (recordedByUserId == Guid.Empty) return Error.Validation("Recorder is required.");
        if (string.IsNullOrWhiteSpace(vaccineName)) return Error.Validation("Vaccine name is required.");
        if (doseNumber < 1) return Error.Validation("Dose number must be at least 1.");
        if (nextDueDate is not null && nextDueDate < administeredDate.Date)
            return Error.Validation("Next-due date cannot be before the administration date.");

        return new Immunization(
            Guid.NewGuid(), facilityId, patientId, vaccineName.Trim(), doseNumber,
            administeredDate, nextDueDate, lotNumber?.Trim(), site?.Trim(), notes,
            recordedByUserId, recordedAtUtc);
    }
}
