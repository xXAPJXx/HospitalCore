using HospitalCore.Modules.Patients.Application.RegisterPatient;
using HospitalCore.Modules.Patients.Domain;
using HospitalCore.SharedKernel;
using Microsoft.Extensions.Time.Testing;

namespace HospitalCore.Modules.Patients.UnitTests.Application;

public sealed class RegisterPatientCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryPatientRepository _repository = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly RegisterPatientCommandHandler _handler;

    public RegisterPatientCommandHandlerTests()
    {
        _handler = new RegisterPatientCommandHandler(_repository, _unitOfWork, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task Handle_WithValidCommand_PersistsPatientAndReturnsItsId()
    {
        var result = await _handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var patient = Assert.Single(_repository.Patients);
        Assert.Equal(patient.Id.Value, result.Value);
        Assert.Equal(Now, patient.RegisteredAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WithDocumentAlreadyRegistered_ReturnsConflictAndDoesNotSave()
    {
        await _handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

        var duplicate = ValidCommand() with { GivenName = "Ana", DocumentNumber = "12.345.678-z" };
        var result = await _handler.Handle(duplicate, TestContext.Current.CancellationToken);

        Assert.Equal(PatientErrors.DocumentAlreadyRegistered, result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(_repository.Patients);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenSameDocumentIsRegisteredConcurrently_ReturnsConflict()
    {
        var handler = new RegisterPatientCommandHandler(_repository, new ConflictingUnitOfWork(), new FakeTimeProvider(Now));

        var result = await handler.Handle(ValidCommand(), TestContext.Current.CancellationToken);

        Assert.Equal(PatientErrors.DocumentAlreadyRegistered, result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithInvalidName_ReturnsValidationErrorAndDoesNotSave()
    {
        var command = ValidCommand() with { GivenName = "   " };

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(PatientErrors.GivenNameRequired, result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(_repository.Patients);
        Assert.Equal(0, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WithDateOfBirthAfterTodayAccordingToTimeProvider_ReturnsDateOfBirthInFuture()
    {
        var command = ValidCommand() with { DateOfBirth = new DateOnly(2026, 11, 26) };

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(PatientErrors.DateOfBirthInFuture, result.Error);
        Assert.Equal(0, _unitOfWork.SaveChangesCalls);
    }

    private static RegisterPatientCommand ValidCommand() => new(
        GivenName: "María",
        FamilyName: "García López",
        DateOfBirth: new DateOnly(1985, 3, 14),
        Sex: AdministrativeSex.Female,
        DocumentType: IdentityDocumentType.NationalId,
        DocumentNumber: "12345678Z");
}
