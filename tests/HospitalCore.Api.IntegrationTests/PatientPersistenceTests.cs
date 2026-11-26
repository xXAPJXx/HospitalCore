using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalCore.Api.IntegrationTests;

public sealed class PatientPersistenceTests(HospitalCoreApiFactory factory)
{
    [Fact]
    public async Task SaveChanges_WithIdentityDocumentAlreadyStored_ThrowsUniqueConstraintViolation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var document = IdentityDocument.Create(IdentityDocumentType.Passport, Guid.NewGuid().ToString("N")[..12]).Value;

        await using (var firstRequest = factory.Services.CreateAsyncScope())
        {
            firstRequest.ServiceProvider.GetRequiredService<IPatientRepository>().Add(NewPatient(document));
            await firstRequest.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        }

        await using var secondRequest = factory.Services.CreateAsyncScope();
        secondRequest.ServiceProvider.GetRequiredService<IPatientRepository>().Add(NewPatient(document));
        var unitOfWork = secondRequest.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => unitOfWork.SaveChangesAsync(cancellationToken));
    }

    private static Patient NewPatient(IdentityDocument document) => Patient.Register(
        PersonName.Create("Test", "Patient").Value,
        new DateOnly(1970, 1, 1),
        AdministrativeSex.Unknown,
        document,
        DateTimeOffset.UtcNow).Value;
}
