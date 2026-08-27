using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>A ward note written during an admission.</summary>
public sealed class WardNote : Entity<Guid>
{
    private WardNote() { } // EF

    internal WardNote(Guid id, string content, Guid authorUserId, DateTime recordedAtUtc)
        : base(id)
    {
        Content = content;
        AuthorUserId = authorUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public string Content { get; private set; } = string.Empty;
    public Guid AuthorUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    internal static Result<WardNote> Create(string content, Guid authorUserId, DateTime recordedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(content)) return Error.Validation("Ward note content is required.");
        return new WardNote(Guid.NewGuid(), content.Trim(), authorUserId, recordedAtUtc);
    }
}
