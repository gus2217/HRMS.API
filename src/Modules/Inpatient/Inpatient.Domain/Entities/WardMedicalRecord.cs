using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>
/// A day-to-day ward medical record (SOAP-style progress note with vitals and
/// optional media attachments) recorded during an admission. The discharge gate
/// requires at least one complete record (assessment + plan) before a patient can
/// be discharged.
/// </summary>
public sealed class WardMedicalRecord : Entity<Guid>
{
    private readonly List<WardRecordAttachment> _attachments = new();

    private WardMedicalRecord() { } // EF

    internal WardMedicalRecord(
        Guid id,
        Guid admissionId,
        Guid recordedByUserId,
        DateTime recordedAtUtc,
        decimal? temperatureCelsius,
        int? systolicBp,
        int? diastolicBp,
        int? pulseRate,
        int? respiratoryRate,
        int? oxygenSaturation,
        decimal? weightKg,
        string? subjective,
        string? objective,
        string? assessment,
        string? plan)
        : base(id)
    {
        AdmissionId = admissionId;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
        TemperatureCelsius = temperatureCelsius;
        SystolicBp = systolicBp;
        DiastolicBp = diastolicBp;
        PulseRate = pulseRate;
        RespiratoryRate = respiratoryRate;
        OxygenSaturation = oxygenSaturation;
        WeightKg = weightKg;
        Subjective = subjective;
        Objective = objective;
        Assessment = assessment;
        Plan = plan;
    }

    public Guid AdmissionId { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    public decimal? TemperatureCelsius { get; private set; }
    public int? SystolicBp { get; private set; }
    public int? DiastolicBp { get; private set; }
    public int? PulseRate { get; private set; }
    public int? RespiratoryRate { get; private set; }
    public int? OxygenSaturation { get; private set; }
    public decimal? WeightKg { get; private set; }

    public string? Subjective { get; private set; }
    public string? Objective { get; private set; }
    public string? Assessment { get; private set; }
    public string? Plan { get; private set; }

    public IReadOnlyCollection<WardRecordAttachment> Attachments => _attachments.AsReadOnly();

    /// <summary>
    /// A record "counts" for the discharge gate only when the clinician has
    /// written both an assessment and a plan (SOAP completion).
    /// </summary>
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Assessment) && !string.IsNullOrWhiteSpace(Plan);

    public static Result<WardMedicalRecord> Create(
        Guid admissionId,
        Guid recordedByUserId,
        DateTime recordedAtUtc,
        decimal? temperatureCelsius,
        int? systolicBp,
        int? diastolicBp,
        int? pulseRate,
        int? respiratoryRate,
        int? oxygenSaturation,
        decimal? weightKg,
        string? subjective,
        string? objective,
        string? assessment,
        string? plan)
    {
        if (admissionId == Guid.Empty) return Error.Validation("Admission is required.");
        if (recordedByUserId == Guid.Empty) return Error.Validation("Recorder is required.");
        if (temperatureCelsius is < 25m or > 45m)
            return Error.Validation("Temperature is outside a plausible range.");
        if (systolicBp is < 0 or > 400) return Error.Validation("Systolic BP is outside a plausible range.");
        if (diastolicBp is < 0 or > 300) return Error.Validation("Diastolic BP is outside a plausible range.");
        if (pulseRate is < 0 or > 300) return Error.Validation("Pulse rate is outside a plausible range.");
        if (respiratoryRate is < 0 or > 100) return Error.Validation("Respiratory rate is outside a plausible range.");
        if (oxygenSaturation is < 0 or > 100) return Error.Validation("Oxygen saturation is outside a plausible range.");
        if (weightKg is < 0 or > 500) return Error.Validation("Weight is outside a plausible range.");
        if (assessment is { Length: > 8000 }) return Error.Validation("Assessment is too long.");
        if (plan is { Length: > 8000 }) return Error.Validation("Plan is too long.");

        return new WardMedicalRecord(
            Guid.NewGuid(), admissionId, recordedByUserId, recordedAtUtc,
            temperatureCelsius, systolicBp, diastolicBp, pulseRate, respiratoryRate,
            oxygenSaturation, weightKg, Trim(subjective), Trim(objective),
            Trim(assessment), Trim(plan));
    }

    public Result Attach(WardRecordAttachment attachment)
    {
        _attachments.Add(attachment);
        return Result.Success();
    }

    private static string? Trim(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>Media attached to a ward medical record (stored via IFileStorage).</summary>
public sealed class WardRecordAttachment : Entity<Guid>
{
    private WardRecordAttachment() { } // EF

    internal WardRecordAttachment(
        Guid id, Guid wardMedicalRecordId, string fileName, string contentType,
        long sizeBytes, string storageKey, Guid uploadedByUserId, DateTime uploadedAtUtc)
        : base(id)
    {
        WardMedicalRecordId = wardMedicalRecordId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        StorageKey = storageKey;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid WardMedicalRecordId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }

    public static Result<WardRecordAttachment> Create(
        Guid wardMedicalRecordId, string fileName, string contentType,
        long sizeBytes, string storageKey, Guid uploadedByUserId, DateTime uploadedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return Error.Validation("File name is required.");
        if (string.IsNullOrWhiteSpace(storageKey)) return Error.Validation("Storage key is required.");
        return new WardRecordAttachment(
            Guid.NewGuid(), wardMedicalRecordId, fileName.Trim(), contentType,
            sizeBytes, storageKey, uploadedByUserId, uploadedAtUtc);
    }
}
