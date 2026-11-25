using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;
using Microsoft.EntityFrameworkCore;

namespace HospitalCore.Modules.Patients.Infrastructure.Persistence;

internal sealed class PatientRepository(PatientsDbContext context) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(PatientId id, CancellationToken cancellationToken) =>
        context.Patients.SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);

    public Task<bool> ExistsWithDocumentAsync(IdentityDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        return context.Patients.AnyAsync(
            patient => patient.Document.Type == document.Type && patient.Document.Number == document.Number,
            cancellationToken);
    }

    public void Add(Patient patient) => context.Patients.Add(patient);
}
