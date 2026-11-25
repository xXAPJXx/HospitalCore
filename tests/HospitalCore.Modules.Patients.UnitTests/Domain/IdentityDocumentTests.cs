using HospitalCore.Modules.Patients.Domain;

namespace HospitalCore.Modules.Patients.UnitTests.Domain;

public sealed class IdentityDocumentTests
{
    [Theory]
    [InlineData("12.345.678-z", "12345678Z")]
    [InlineData(" x1234567l ", "X1234567L")]
    [InlineData("AB 123 456", "AB123456")]
    public void Create_NormalizesSeparatorsAndCase(string input, string expected)
    {
        var result = IdentityDocument.Create(IdentityDocumentType.NationalId, input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Number);
        Assert.Equal(IdentityDocumentType.NationalId, result.Value.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" - . ")]
    public void Create_WithoutNumber_ReturnsDocumentNumberRequired(string? number)
    {
        var result = IdentityDocument.Create(IdentityDocumentType.Passport, number);

        Assert.Equal(PatientErrors.DocumentNumberRequired, result.Error);
    }

    [Theory]
    [InlineData("1234/5678")]
    [InlineData("1234Ñ")]
    public void Create_WithCharactersOtherThanAsciiLettersAndDigits_ReturnsDocumentNumberInvalid(string number)
    {
        var result = IdentityDocument.Create(IdentityDocumentType.Passport, number);

        Assert.Equal(PatientErrors.DocumentNumberInvalid, result.Error);
    }

    [Fact]
    public void Create_WithNumberAboveMaxLength_ReturnsDocumentNumberTooLong()
    {
        var result = IdentityDocument.Create(IdentityDocumentType.Other, new string('1', IdentityDocument.MaxNumberLength + 1));

        Assert.Equal(PatientErrors.DocumentNumberTooLong, result.Error);
    }

    [Fact]
    public void Create_WithUndefinedType_ReturnsDocumentTypeInvalid()
    {
        var result = IdentityDocument.Create((IdentityDocumentType)99, "12345678Z");

        Assert.Equal(PatientErrors.DocumentTypeInvalid, result.Error);
    }
}
