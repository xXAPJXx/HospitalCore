using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.Application.RegisterPatient;

public sealed record RegisterPatientCommand(
    string GivenName,
    string FamilyName,
    DateOnly DateOfBirth,
    AdministrativeSex Sex,
    IdentityDocumentType DocumentType,
    string DocumentNumber);
