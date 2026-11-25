using HospitalCore.SharedKernel;

namespace HospitalCore.Modules.Patients.Domain;

public static class PatientErrors
{
    public static readonly Error GivenNameRequired =
        Error.Validation("Patients.GivenNameRequired", "The given name is required.");

    public static readonly Error GivenNameTooLong =
        Error.Validation("Patients.GivenNameTooLong", $"The given name cannot exceed {PersonName.MaxLength} characters.");

    public static readonly Error FamilyNameRequired =
        Error.Validation("Patients.FamilyNameRequired", "The family name is required.");

    public static readonly Error FamilyNameTooLong =
        Error.Validation("Patients.FamilyNameTooLong", $"The family name cannot exceed {PersonName.MaxLength} characters.");

    public static readonly Error DocumentTypeInvalid =
        Error.Validation("Patients.DocumentTypeInvalid", "The identity document type is not valid.");

    public static readonly Error DocumentNumberRequired =
        Error.Validation("Patients.DocumentNumberRequired", "The identity document number is required.");

    public static readonly Error DocumentNumberTooLong =
        Error.Validation("Patients.DocumentNumberTooLong", $"The identity document number cannot exceed {IdentityDocument.MaxNumberLength} characters.");

    public static readonly Error DocumentNumberInvalid =
        Error.Validation("Patients.DocumentNumberInvalid", "The identity document number can only contain letters and digits.");

    public static readonly Error DateOfBirthInFuture =
        Error.Validation("Patients.DateOfBirthInFuture", "The date of birth cannot be in the future.");

    public static readonly Error DateOfBirthTooOld =
        Error.Validation("Patients.DateOfBirthTooOld", $"The date of birth cannot be more than {Patient.MaxAgeInYears} years ago.");

    public static readonly Error SexInvalid =
        Error.Validation("Patients.SexInvalid", "The administrative sex is not valid.");

    public static readonly Error DocumentAlreadyRegistered =
        Error.Conflict("Patients.DocumentAlreadyRegistered", "A patient with the same identity document is already registered.");

    public static Error NotFound(PatientId id) =>
        Error.NotFound("Patients.NotFound", $"The patient '{id.Value}' was not found.");
}
