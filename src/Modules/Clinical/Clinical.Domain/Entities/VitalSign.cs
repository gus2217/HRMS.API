using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A standalone vital-signs observation recorded against a patient, independent of
/// any single consultation. Unlike triage (which is tied to a visit), these
/// observations accumulate over time so clinicians can see a trend (temperature,
/// blood pressure, pulse, respiration, SpO2, weight, height, BMI). Mirrors the
/// OpenMRS "Vitals & Biometrics" patient-chart widget.
/// </summary>
public sealed class VitalSign : AggregateRoot<Guid>
{
    private VitalSign() { } // EF

    private VitalSign(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        decimal? temperatureCelsius,
        int? systolicBp,
        int? diastolicBp,
        int? pulseRate,
        int? respiratoryRate,
        int? oxygenSaturation,
        decimal? weightKg,
        decimal? heightCm,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        TemperatureCelsius = temperatureCelsius;
        SystolicBp = systolicBp;
        DiastolicBp = diastolicBp;
        PulseRate = pulseRate;
        RespiratoryRate = respiratoryRate;
        OxygenSaturation = oxygenSaturation;
        WeightKg = weightKg;
        HeightCm = heightCm;
        Bmi = CalculateBmi(weightKg, heightCm);
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public decimal? TemperatureCelsius { get; private set; }
    public int? SystolicBp { get; private set; }
    public int? DiastolicBp { get; private set; }
    public int? PulseRate { get; private set; }
    public int? RespiratoryRate { get; private set; }
    public int? OxygenSaturation { get; private set; }
    public decimal? WeightKg { get; private set; }
    public decimal? HeightCm { get; private set; }
    public decimal? Bmi { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    public static Result<VitalSign> Record(
        FacilityId facilityId,
        Guid patientId,
        decimal? temperatureCelsius,
        int? systolicBp,
        int? diastolicBp,
        int? pulseRate,
        int? respiratoryRate,
        int? oxygenSaturation,
        decimal? weightKg,
        decimal? heightCm,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (recordedByUserId == Guid.Empty) return Error.Validation("Recorder is required.");

        if (temperatureCelsius is < 25m or > 45m)
            return Error.Validation("Temperature is outside a plausible range.");
        if (systolicBp is < 0 or > 400) return Error.Validation("Systolic BP is outside a plausible range.");
        if (diastolicBp is < 0 or > 300) return Error.Validation("Diastolic BP is outside a plausible range.");
        if (pulseRate is < 0 or > 300) return Error.Validation("Pulse rate is outside a plausible range.");
        if (respiratoryRate is < 0 or > 100) return Error.Validation("Respiratory rate is outside a plausible range.");
        if (oxygenSaturation is < 0 or > 100) return Error.Validation("Oxygen saturation is outside a plausible range.");
        if (weightKg is < 0 or > 500) return Error.Validation("Weight is outside a plausible range.");
        if (heightCm is < 0 or > 300) return Error.Validation("Height is outside a plausible range.");

        return new VitalSign(
            Guid.NewGuid(), facilityId, patientId,
            temperatureCelsius, systolicBp, diastolicBp,
            pulseRate, respiratoryRate, oxygenSaturation,
            weightKg, heightCm, recordedByUserId, recordedAtUtc);
    }

    private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
    {
        if (weightKg is null || heightCm is null || heightCm == 0m) return null;
        var heightM = heightCm.Value / 100m;
        return Math.Round(weightKg.Value / (heightM * heightM), 1);
    }
}
