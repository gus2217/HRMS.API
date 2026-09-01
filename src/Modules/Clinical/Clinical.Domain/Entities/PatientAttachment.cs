using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A document attached to a patient's record (scan, lab report, referral letter,
/// consent form). The binary content is stored out-of-band via <c>IFileStorage</c>;
/// this entity holds only metadata plus the storage key. Mirrors the OpenMRS
/// "Attachments" patient-chart widget.
/// </summary>
public sealed class PatientAttachment : AggregateRoot<Guid>
{
    private PatientAttachment() { } // EF

    private PatientAttachment(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        string fileName,
        string contentType,
        long sizeBytes,
        string category,
        string storageKey,
        Guid uploadedByUserId,
        DateTime uploadedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Category = category;
        StorageKey = storageKey;
        UploadedByUserId = uploadedByUserId;
        UploadedAtUtc = uploadedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }

    public static Result<PatientAttachment> Create(
        FacilityId facilityId,
        Guid patientId,
        string fileName,
        string contentType,
        long sizeBytes,
        string category,
        string storageKey,
        Guid uploadedByUserId,
        DateTime uploadedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (uploadedByUserId == Guid.Empty) return Error.Validation("Uploader is required.");
        if (string.IsNullOrWhiteSpace(fileName)) return Error.Validation("File name is required.");
        if (string.IsNullOrWhiteSpace(storageKey)) return Error.Validation("Storage key is required.");
        if (sizeBytes < 0) return Error.Validation("File size cannot be negative.");

        return new PatientAttachment(
            Guid.NewGuid(), facilityId, patientId, fileName.Trim(), contentType,
            sizeBytes, category.Trim(), storageKey, uploadedByUserId, uploadedAtUtc);
    }
}
