using HospitalCore.SharedKernel;

namespace HospitalCore.Modules.Patients.Domain;

public sealed record PersonName
{
    public const int MaxLength = 100;

    private PersonName(string givenName, string familyName)
    {
        GivenName = givenName;
        FamilyName = familyName;
    }

    public string GivenName { get; }

    public string FamilyName { get; }

    public static Result<PersonName> Create(string? givenName, string? familyName)
    {
        var given = CollapseWhitespace(givenName);
        var family = CollapseWhitespace(familyName);

        if (given.Length == 0)
        {
            return Result.Failure<PersonName>(PatientErrors.GivenNameRequired);
        }

        if (given.Length > MaxLength)
        {
            return Result.Failure<PersonName>(PatientErrors.GivenNameTooLong);
        }

        if (family.Length == 0)
        {
            return Result.Failure<PersonName>(PatientErrors.FamilyNameRequired);
        }

        if (family.Length > MaxLength)
        {
            return Result.Failure<PersonName>(PatientErrors.FamilyNameTooLong);
        }

        return Result.Success(new PersonName(given, family));
    }

    private static string CollapseWhitespace(string? value) =>
        string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
