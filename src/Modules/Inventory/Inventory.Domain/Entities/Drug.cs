using Jacana.SharedKernel.Domain;

namespace Jacana.Inventory.Domain;

/// <summary>
/// A drug catalog item. Pricing and reorder threshold live here; physical stock is
/// tracked separately in <see cref="StockBatch"/>.
/// </summary>
public sealed class Drug : AggregateRoot<Guid>
{
    private Drug() { } // EF

    private Drug(Guid id, FacilityId facilityId, string code, string name, string category, string form,
        Money unitPrice, int reorderLevel)
        : base(id)
    {
        FacilityId = facilityId;
        Code = code;
        Name = name;
        Category = category;
        Form = form;
        UnitPrice = unitPrice;
        ReorderLevel = reorderLevel;
        Status = RecordStatus.Active;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    /// <summary>Clinical/therapeutic class — the "type" of drug (e.g. Antibiotic, Analgesic).</summary>
    public string Category { get; private set; } = string.Empty;
    public string Form { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = null!;
    public int ReorderLevel { get; private set; }
    public RecordStatus Status { get; private set; }

    public static Result<Drug> Create(
        Guid id, FacilityId facilityId, string code, string name, string category, string form,
        Money unitPrice, int reorderLevel)
    {
        if (string.IsNullOrWhiteSpace(code)) return Error.Validation("Drug code is required.");
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Drug name is required.");
        if (string.IsNullOrWhiteSpace(category)) return Error.Validation("Drug category is required.");
        if (string.IsNullOrWhiteSpace(form)) return Error.Validation("Drug form is required.");
        if (reorderLevel < 0) return Error.Validation("Reorder level cannot be negative.");
        return new Drug(id, facilityId, code.Trim().ToUpperInvariant(), name.Trim(), category.Trim(), form.Trim(), unitPrice, reorderLevel);
    }

    public Result UpdateCatalog(string name, string category, string form, Money unitPrice, int reorderLevel)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Drug name is required.");
        if (string.IsNullOrWhiteSpace(category)) return Error.Validation("Drug category is required.");
        if (string.IsNullOrWhiteSpace(form)) return Error.Validation("Drug form is required.");
        if (reorderLevel < 0) return Error.Validation("Reorder level cannot be negative.");
        Name = name.Trim();
        Category = category.Trim();
        Form = form.Trim();
        UnitPrice = unitPrice;
        ReorderLevel = reorderLevel;
        return Result.Success();
    }
}
