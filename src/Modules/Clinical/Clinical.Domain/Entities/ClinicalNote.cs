using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

public sealed class ClinicalNote : Entity<Guid>
{
    private ClinicalNote() { } // EF

    internal ClinicalNote(Guid id, string content, Guid authorUserId, DateTime recordedAtUtc)
        : base(id)
    {
        Content = content;
        AuthorUserId = authorUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public string Content { get; private set; } = string.Empty;
    public Guid AuthorUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    internal static Result<ClinicalNote> Create(string content, Guid authorUserId, DateTime recordedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(content)) return Error.Validation("Clinical note content is required.");
        return new ClinicalNote(Guid.NewGuid(), content.Trim(), authorUserId, recordedAtUtc);
    }
}
