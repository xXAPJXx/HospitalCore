using HospitalCore.SharedKernel;

namespace HospitalCore.Modules.Patients.Domain;

public sealed class Patient : Entity<PatientId>
{
    public const int MaxAgeInYears = 150;

    private Patient(
        PatientId id,
        PersonName name,
        DateOnly dateOfBirth,
        AdministrativeSex sex,
        IdentityDocument document,
        DateTimeOffset registeredAtUtc)
        : base(id)
    {
        Name = name;
        DateOfBirth = dateOfBirth;
        Sex = sex;
        Document = document;
        RegisteredAtUtc = registeredAtUtc;
    }

    // Required by Entity Framework Core to materialize the aggregate.
    private Patient()
        : base(default)
    {
        Name = null!;
        Document = null!;
    }

    public PersonName Name { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public AdministrativeSex Sex { get; private set; }

    public IdentityDocument Document { get; private set; }

    public DateTimeOffset RegisteredAtUtc { get; private set; }

    /// <summary>
    /// Registers a new patient.
    /// </summary>
    /// <param name="now">Current date and time in the hospital's local time zone, used to validate the date of birth.</param>
    public static Result<Patient> Register(
        PersonName name,
        DateOnly dateOfBirth,
        AdministrativeSex sex,
        IdentityDocument document,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);

        var today = DateOnly.FromDateTime(now.DateTime);

        if (dateOfBirth > today)
        {
            return Result.Failure<Patient>(PatientErrors.DateOfBirthInFuture);
        }

        if (dateOfBirth < today.AddYears(-MaxAgeInYears))
        {
            return Result.Failure<Patient>(PatientErrors.DateOfBirthTooOld);
        }

        if (!Enum.IsDefined(sex))
        {
            return Result.Failure<Patient>(PatientErrors.SexInvalid);
        }

        var patient = new Patient(PatientId.New(), name, dateOfBirth, sex, document, now.ToUniversalTime());
        return Result.Success(patient);
    }
}
