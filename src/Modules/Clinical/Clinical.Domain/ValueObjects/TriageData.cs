using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>Vital-sign triage measurements captured at registration. All fields optional.</summary>
public sealed class TriageData : ValueObject
{
    private TriageData(decimal? temperatureCelsius, string? bloodPressure, int? pulseRate,
        int? respiratoryRate, decimal? weightKg)
    {
        TemperatureCelsius = temperatureCelsius;
        BloodPressure = bloodPressure;
        PulseRate = pulseRate;
        RespiratoryRate = respiratoryRate;
        WeightKg = weightKg;
    }

    public decimal? TemperatureCelsius { get; }
    public string? BloodPressure { get; }
    public int? PulseRate { get; }
    public int? RespiratoryRate { get; }
    public decimal? WeightKg { get; }

    public static Result<TriageData> Create(
        decimal? temperatureCelsius, string? bloodPressure, int? pulseRate,
        int? respiratoryRate, decimal? weightKg)
    {
        if (temperatureCelsius is < 25m or > 45m)
            return Error.Validation("Temperature is outside a plausible range.");
        if (pulseRate is < 0 or > 300)
            return Error.Validation("Pulse rate is outside a plausible range.");
        if (respiratoryRate is < 0 or > 100)
            return Error.Validation("Respiratory rate is outside a plausible range.");
        if (weightKg is < 0 or > 500)
            return Error.Validation("Weight is outside a plausible range.");

        return new TriageData(temperatureCelsius, bloodPressure, pulseRate, respiratoryRate, weightKg);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TemperatureCelsius;
        yield return BloodPressure;
        yield return PulseRate;
        yield return RespiratoryRate;
        yield return WeightKg;
    }
}
