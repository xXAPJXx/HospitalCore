using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Api.Controllers.Patients;

public sealed record RegisterPatientRequest(
    string GivenName,
    string FamilyName,
    DateOnly DateOfBirth,
    AdministrativeSex Sex,
    IdentityDocumentType DocumentType,
    string DocumentNumber);

public sealed record RegisterPatientResponse(Guid Id);
