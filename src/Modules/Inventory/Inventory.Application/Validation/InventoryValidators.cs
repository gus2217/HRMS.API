using FluentValidation;
using Jacana.Inventory.Application.Features.Inventory;

namespace Jacana.Inventory.Application.Validation;

public sealed class CreateDrugCommandValidator : AbstractValidator<CreateDrugCommand>
{
    public CreateDrugCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Form).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
    }
}

public sealed class ReceiveStockCommandValidator : AbstractValidator<ReceiveStockCommand>
{
    public ReceiveStockCommandValidator()
    {
        RuleFor(x => x.DrugId).NotEmpty();
        RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.StockBatchId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetDrugCatalogQueryValidator : AbstractValidator<GetDrugCatalogQuery>
{
    public GetDrugCatalogQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
