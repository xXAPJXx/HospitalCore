using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.UnitTests.Application;

internal sealed class InMemoryPatientRepository : IPatientRepository
{
    public List<Patient> Patients { get; } = [];

    public Task<Patient?> GetByIdAsync(PatientId id, CancellationToken cancellationToken) =>
        Task.FromResult(Patients.SingleOrDefault(patient => patient.Id == id));

    public Task<bool> ExistsWithDocumentAsync(IdentityDocument document, CancellationToken cancellationToken) =>
        Task.FromResult(Patients.Any(patient => patient.Document == document));

    public void Add(Patient patient) => Patients.Add(patient);
}

internal sealed class CountingUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
