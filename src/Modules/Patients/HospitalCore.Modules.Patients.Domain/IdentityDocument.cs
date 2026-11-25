using HospitalCore.SharedKernel;

namespace HospitalCore.Modules.Patients.Domain;

public sealed record IdentityDocument
{
    public const int MaxNumberLength = 20;

    private static readonly char[] Separators = [' ', '-', '.'];

    private IdentityDocument(IdentityDocumentType type, string number)
    {
        Type = type;
        Number = number;
    }

    public IdentityDocumentType Type { get; }

    public string Number { get; }

    public static Result<IdentityDocument> Create(IdentityDocumentType type, string? number)
    {
        if (!Enum.IsDefined(type))
        {
            return Result.Failure<IdentityDocument>(PatientErrors.DocumentTypeInvalid);
        }

        var normalized = string.Concat((number ?? string.Empty).Split(Separators, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();

        if (normalized.Length == 0)
        {
            return Result.Failure<IdentityDocument>(PatientErrors.DocumentNumberRequired);
        }

        if (normalized.Length > MaxNumberLength)
        {
            return Result.Failure<IdentityDocument>(PatientErrors.DocumentNumberTooLong);
        }

        if (!normalized.All(char.IsAsciiLetterOrDigit))
        {
            return Result.Failure<IdentityDocument>(PatientErrors.DocumentNumberInvalid);
        }

        return Result.Success(new IdentityDocument(type, normalized));
    }
}
