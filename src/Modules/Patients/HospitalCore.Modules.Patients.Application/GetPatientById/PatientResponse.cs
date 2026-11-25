using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.Application.GetPatientById;

public sealed record PatientResponse(
    Guid Id,
    string GivenName,
    string FamilyName,
    DateOnly DateOfBirth,
    AdministrativeSex Sex,
    IdentityDocumentType DocumentType,
    string DocumentNumber,
    DateTimeOffset RegisteredAtUtc)
{
    internal static PatientResponse FromPatient(Patient patient) => new(
        patient.Id.Value,
        patient.Name.GivenName,
        patient.Name.FamilyName,
        patient.DateOfBirth,
        patient.Sex,
        patient.Document.Type,
        patient.Document.Number,
        patient.RegisteredAtUtc);
}
