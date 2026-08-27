using Jacana.PatientRegistration.Infrastructure.Persistence;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Infrastructure;
using Jacana.SharedKernel.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacana.PatientRegistration.Infrastructure;

/// <summary>Design-time factory for `dotnet ef migrations` (no running app required).</summary>
public sealed class PatientDbContextFactory : IDesignTimeDbContextFactory<PatientDbContext>
{
    public PatientDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PatientDbContext>()
            .UseNpgsql(DesignTime.ConnectionString)
            .Options;
        var encryptor = (IValueEncryptor)new AesGcmValueEncryptor(DesignTime.DevEncryptionKey);
        return new PatientDbContext(options, encryptor);
    }
}
