using HospitalCore.Modules.Patients.Application.GetPatientById;
using HospitalCore.Modules.Patients.Domain;
using HospitalCore.SharedKernel;

namespace HospitalCore.Modules.Patients.UnitTests.Application;

public sealed class GetPatientByIdQueryHandlerTests
{
    private readonly InMemoryPatientRepository _repository = new();
    private readonly GetPatientByIdQueryHandler _handler;

    public GetPatientByIdQueryHandlerTests()
    {
        _handler = new GetPatientByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingPatient_ReturnsMappedResponse()
    {
        var patient = Patient.Register(
            PersonName.Create("María", "García López").Value,
            new DateOnly(1985, 3, 14),
            AdministrativeSex.Female,
            IdentityDocument.Create(IdentityDocumentType.Passport, "AB123456").Value,
            new DateTimeOffset(2026, 11, 25, 9, 0, 0, TimeSpan.Zero)).Value;
        _repository.Add(patient);

        var result = await _handler.Handle(new GetPatientByIdQuery(patient.Id.Value), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var response = result.Value;
        Assert.Equal(patient.Id.Value, response.Id);
        Assert.Equal("María", response.GivenName);
        Assert.Equal("García López", response.FamilyName);
        Assert.Equal(new DateOnly(1985, 3, 14), response.DateOfBirth);
        Assert.Equal(AdministrativeSex.Female, response.Sex);
        Assert.Equal(IdentityDocumentType.Passport, response.DocumentType);
        Assert.Equal("AB123456", response.DocumentNumber);
    }

    [Fact]
    public async Task Handle_WithUnknownPatient_ReturnsNotFound()
    {
        var result = await _handler.Handle(new GetPatientByIdQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Patients.NotFound", result.Error.Code);
    }
}
