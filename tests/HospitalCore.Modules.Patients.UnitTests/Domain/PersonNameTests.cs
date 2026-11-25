using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.UnitTests.Domain;

public sealed class PersonNameTests
{
    [Fact]
    public void Create_WithValidNames_CollapsesWhitespace()
    {
        var result = PersonName.Create("  María   José ", " García  López ");

        Assert.True(result.IsSuccess);
        Assert.Equal("María José", result.Value.GivenName);
        Assert.Equal("García López", result.Value.FamilyName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutGivenName_ReturnsGivenNameRequired(string? givenName)
    {
        var result = PersonName.Create(givenName, "García");

        Assert.Equal(PatientErrors.GivenNameRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutFamilyName_ReturnsFamilyNameRequired(string? familyName)
    {
        var result = PersonName.Create("María", familyName);

        Assert.Equal(PatientErrors.FamilyNameRequired, result.Error);
    }

    [Fact]
    public void Create_WithGivenNameAboveMaxLength_ReturnsGivenNameTooLong()
    {
        var result = PersonName.Create(new string('a', PersonName.MaxLength + 1), "García");

        Assert.Equal(PatientErrors.GivenNameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithFamilyNameAboveMaxLength_ReturnsFamilyNameTooLong()
    {
        var result = PersonName.Create("María", new string('a', PersonName.MaxLength + 1));

        Assert.Equal(PatientErrors.FamilyNameTooLong, result.Error);
    }

    [Fact]
    public void TwoNamesWithSameValues_AreEqual()
    {
        var first = PersonName.Create("María", "García").Value;
        var second = PersonName.Create(" María ", "García").Value;

        Assert.Equal(first, second);
    }
}
