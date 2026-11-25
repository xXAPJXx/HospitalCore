using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.Application.Abstractions;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(PatientId id, CancellationToken cancellationToken);

    Task<bool> ExistsWithDocumentAsync(IdentityDocument document, CancellationToken cancellationToken);

    void Add(Patient patient);
}
