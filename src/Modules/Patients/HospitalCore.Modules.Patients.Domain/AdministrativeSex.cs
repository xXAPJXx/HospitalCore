namespace HospitalCore.Modules.Patients.Domain;

/// <summary>
/// Administrative sex used for registration and record-keeping purposes, aligned with the HL7 FHIR AdministrativeGender value set.
/// </summary>
public enum AdministrativeSex
{
    Unknown = 0,
    Female = 1,
    Male = 2,
    Other = 3,
}
