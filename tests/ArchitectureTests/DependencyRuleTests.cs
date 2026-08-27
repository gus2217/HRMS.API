using NetArchTest.Rules;
using Xunit;

namespace Jacana.Tests.Architecture;

/// <summary>
/// Architecture enforcement from the first commit. These rules encode the
/// non-negotiable dependency rules in the spec — they fail the build, not a review.
/// </summary>
public class DependencyRuleTests
{
    private static readonly Types Domain = Types.InAssembly(typeof(Jacana.SharedKernel.Domain.Entity<Guid>).Assembly);
    private static readonly Types IdentityDomain = Types.InAssembly(typeof(Jacana.Identity.Domain.User).Assembly);
    private static readonly Types IdentityApplication = Types.InAssembly(typeof(Jacana.Identity.Application.Features.Auth.LoginCommand).Assembly);
    private static readonly Types Api = Types.InAssembly(typeof(Jacana.HRMS.Api.Endpoints.IdentityEndpoints).Assembly);
    private static readonly Types PatientDomain = Types.InAssembly(typeof(Jacana.PatientRegistration.Domain.Patient).Assembly);
    private static readonly Types PatientApplication = Types.InAssembly(typeof(Jacana.PatientRegistration.Application.Features.Patients.RegisterPatientCommand).Assembly);
    private static readonly Types ClinicalDomain = Types.InAssembly(typeof(Jacana.Clinical.Domain.Consultation).Assembly);
    private static readonly Types ClinicalApplication = Types.InAssembly(typeof(Jacana.Clinical.Application.Features.Consultations.StartConsultationCommand).Assembly);
    private static readonly Types InventoryDomain = Types.InAssembly(typeof(Jacana.Inventory.Domain.Drug).Assembly);
    private static readonly Types InventoryApplication = Types.InAssembly(typeof(Jacana.Inventory.Application.Features.Inventory.CreateDrugCommand).Assembly);
    private static readonly Types PharmacyDomain = Types.InAssembly(typeof(Jacana.Pharmacy.Domain.Prescription).Assembly);
    private static readonly Types PharmacyApplication = Types.InAssembly(typeof(Jacana.Pharmacy.Application.Features.Pharmacy.CreatePrescriptionCommand).Assembly);
    private static readonly Types LaboratoryDomain = Types.InAssembly(typeof(Jacana.Laboratory.Domain.LabOrder).Assembly);
    private static readonly Types LaboratoryApplication = Types.InAssembly(typeof(Jacana.Laboratory.Application.Features.Laboratory.CreateLabOrderCommand).Assembly);
    private static readonly Types BillingDomain = Types.InAssembly(typeof(Jacana.Billing.Domain.Invoice).Assembly);
    private static readonly Types BillingApplication = Types.InAssembly(typeof(Jacana.Billing.Application.Features.Billing.IssueInvoiceCommand).Assembly);
    private static readonly Types InpatientDomain = Types.InAssembly(typeof(Jacana.Inpatient.Domain.Admission).Assembly);
    private static readonly Types InpatientApplication = Types.InAssembly(typeof(Jacana.Inpatient.Application.Features.Inpatient.AdmitPatientCommand).Assembly);
    private static readonly Types NotificationsDomain = Types.InAssembly(typeof(Jacana.Notifications.Domain.NotificationMessage).Assembly);

    [Fact]
    public void Domain_has_no_dependency_on_ef_or_http()
    {
        var result = Domain.That()
            .ResideInNamespace("Jacana.SharedKernel.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "System.Net.Http",
                "MediatR",
                "FluentValidation")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void IdentityDomain_references_only_shared_kernel_and_bcl()
    {
        var result = IdentityDomain.That()
            .ResideInNamespace("Jacana.Identity.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Jacana.Identity.Application",
                "Jacana.Identity.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void IdentityApplication_does_not_reference_infrastructure()
    {
        var result = IdentityApplication.That()
            .ResideInNamespace("Jacana.Identity.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Identity.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void PatientDomain_references_only_shared_kernel_and_bcl()
    {
        var result = PatientDomain.That()
            .ResideInNamespace("Jacana.PatientRegistration.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.PatientRegistration.Application",
                "Jacana.PatientRegistration.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void PatientApplication_does_not_reference_infrastructure()
    {
        var result = PatientApplication.That()
            .ResideInNamespace("Jacana.PatientRegistration.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.PatientRegistration.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ClinicalDomain_references_only_shared_kernel_and_bcl()
    {
        var result = ClinicalDomain.That()
            .ResideInNamespace("Jacana.Clinical.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Clinical.Application",
                "Jacana.Clinical.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ClinicalApplication_does_not_reference_infrastructure()
    {
        var result = ClinicalApplication.That()
            .ResideInNamespace("Jacana.Clinical.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Clinical.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void InventoryDomain_references_only_shared_kernel_and_bcl()
    {
        var result = InventoryDomain.That()
            .ResideInNamespace("Jacana.Inventory.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Inventory.Application",
                "Jacana.Inventory.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void InventoryApplication_does_not_reference_infrastructure()
    {
        var result = InventoryApplication.That()
            .ResideInNamespace("Jacana.Inventory.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Inventory.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void PharmacyDomain_references_only_shared_kernel_and_bcl()
    {
        var result = PharmacyDomain.That()
            .ResideInNamespace("Jacana.Pharmacy.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Pharmacy.Application",
                "Jacana.Pharmacy.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void PharmacyApplication_does_not_reference_infrastructure()
    {
        var result = PharmacyApplication.That()
            .ResideInNamespace("Jacana.Pharmacy.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Pharmacy.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void LaboratoryDomain_references_only_shared_kernel_and_bcl()
    {
        var result = LaboratoryDomain.That()
            .ResideInNamespace("Jacana.Laboratory.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Laboratory.Application",
                "Jacana.Laboratory.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void LaboratoryApplication_does_not_reference_infrastructure()
    {
        var result = LaboratoryApplication.That()
            .ResideInNamespace("Jacana.Laboratory.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Laboratory.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void BillingDomain_references_only_shared_kernel_and_bcl()
    {
        var result = BillingDomain.That()
            .ResideInNamespace("Jacana.Billing.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Billing.Application",
                "Jacana.Billing.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void BillingApplication_does_not_reference_infrastructure()
    {
        var result = BillingApplication.That()
            .ResideInNamespace("Jacana.Billing.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Billing.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void InpatientDomain_references_only_shared_kernel_and_bcl()
    {
        var result = InpatientDomain.That()
            .ResideInNamespace("Jacana.Inpatient.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Inpatient.Application",
                "Jacana.Inpatient.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void InpatientApplication_does_not_reference_infrastructure()
    {
        var result = InpatientApplication.That()
            .ResideInNamespace("Jacana.Inpatient.Application")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Inpatient.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void NotificationsDomain_references_only_shared_kernel_and_bcl()
    {
        var result = NotificationsDomain.That()
            .ResideInNamespace("Jacana.Notifications.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Jacana.Notifications.Application",
                "Jacana.Notifications.Infrastructure",
                "Jacana.SharedKernel.Application",
                "Jacana.SharedKernel.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Api_has_no_business_logic_dependency_on_identity_domain()
    {
        var result = Api.That()
            .ResideInNamespace("Jacana.HRMS.Api")
            .ShouldNot()
            .HaveDependencyOn("Jacana.Identity.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Aggregate_roots_expose_behavior_not_public_setters()
    {
        var aggregates = Types.InAssembly(typeof(Jacana.Identity.Domain.User).Assembly)
            .That()
            .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
            .GetTypes()
            .Concat(Types.InAssembly(typeof(Jacana.PatientRegistration.Domain.Patient).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Clinical.Domain.Consultation).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Inventory.Domain.Drug).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Pharmacy.Domain.Prescription).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Laboratory.Domain.LabOrder).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Billing.Domain.Invoice).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Inpatient.Domain.Admission).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .Concat(Types.InAssembly(typeof(Jacana.Notifications.Domain.NotificationMessage).Assembly)
                .That()
                .Inherit(typeof(Jacana.SharedKernel.Domain.AggregateRoot<Guid>))
                .GetTypes())
            .ToList();

        var badTypes = new List<string>();
        foreach (var type in aggregates)
        {
            var publicSetters = type.GetProperties()
                .Where(p => p.SetMethod?.IsPublic == true)
                .Select(p => p.Name)
                .ToList();
            if (publicSetters.Count > 0)
                badTypes.Add($"{type.Name}: {string.Join(", ", publicSetters)}");
        }

        Assert.Empty(badTypes);
    }
}
