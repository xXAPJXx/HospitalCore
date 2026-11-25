using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.UnitTests.Domain;

public sealed class PatientTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 10, 30, 0, TimeSpan.FromHours(1));
    private static readonly DateOnly Today = new(2026, 11, 25);

    private static readonly PersonName Name = PersonName.Create("María", "García López").Value;
    private static readonly IdentityDocument Document = IdentityDocument.Create(IdentityDocumentType.NationalId, "12345678Z").Value;

    [Fact]
    public void Register_WithValidData_CreatesPatient()
    {
        var dateOfBirth = new DateOnly(1985, 3, 14);

        var result = Patient.Register(Name, dateOfBirth, AdministrativeSex.Female, Document, Now);

        Assert.True(result.IsSuccess);
        var patient = result.Value;
        Assert.NotEqual(Guid.Empty, patient.Id.Value);
        Assert.Equal(Name, patient.Name);
        Assert.Equal(dateOfBirth, patient.DateOfBirth);
        Assert.Equal(AdministrativeSex.Female, patient.Sex);
        Assert.Equal(Document, patient.Document);
        Assert.Equal(Now, patient.RegisteredAtUtc);
        Assert.Equal(TimeSpan.Zero, patient.RegisteredAtUtc.Offset);
    }

    [Fact]
    public void Register_NewbornBornToday_Succeeds()
    {
        var result = Patient.Register(Name, Today, AdministrativeSex.Unknown, Document, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Register_WithDateOfBirthInFuture_ReturnsDateOfBirthInFuture()
    {
        var result = Patient.Register(Name, Today.AddDays(1), AdministrativeSex.Male, Document, Now);

        Assert.Equal(PatientErrors.DateOfBirthInFuture, result.Error);
    }

    [Fact]
    public void Register_WithDateOfBirthExactlyAtMaxAge_Succeeds()
    {
        var result = Patient.Register(Name, Today.AddYears(-Patient.MaxAgeInYears), AdministrativeSex.Male, Document, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Register_WithDateOfBirthBeyondMaxAge_ReturnsDateOfBirthTooOld()
    {
        var dateOfBirth = Today.AddYears(-Patient.MaxAgeInYears).AddDays(-1);

        var result = Patient.Register(Name, dateOfBirth, AdministrativeSex.Male, Document, Now);

        Assert.Equal(PatientErrors.DateOfBirthTooOld, result.Error);
    }

    [Fact]
    public void Register_WithUndefinedSex_ReturnsSexInvalid()
    {
        var result = Patient.Register(Name, new DateOnly(1985, 3, 14), (AdministrativeSex)42, Document, Now);

        Assert.Equal(PatientErrors.SexInvalid, result.Error);
    }

    [Fact]
    public void Register_TwoPatients_GetDifferentIds()
    {
        var first = Patient.Register(Name, new DateOnly(1985, 3, 14), AdministrativeSex.Female, Document, Now).Value;
        var second = Patient.Register(Name, new DateOnly(1985, 3, 14), AdministrativeSex.Female, Document, Now).Value;

        Assert.NotEqual(first.Id, second.Id);
    }
}
