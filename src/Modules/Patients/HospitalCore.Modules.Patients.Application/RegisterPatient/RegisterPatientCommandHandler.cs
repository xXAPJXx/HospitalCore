using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;
using HospitalCore.SharedKernel;
using HospitalCore.SharedKernel.Messaging;

namespace HospitalCore.Modules.Patients.Application.RegisterPatient;

public sealed class RegisterPatientCommandHandler(
    IPatientRepository patients,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RegisterPatientCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterPatientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var name = PersonName.Create(command.GivenName, command.FamilyName);
        if (name.IsFailure)
        {
            return Result.Failure<Guid>(name.Error);
        }

        var document = IdentityDocument.Create(command.DocumentType, command.DocumentNumber);
        if (document.IsFailure)
        {
            return Result.Failure<Guid>(document.Error);
        }

        var registration = Patient.Register(name.Value, command.DateOfBirth, command.Sex, document.Value, timeProvider.GetLocalNow());
        if (registration.IsFailure)
        {
            return Result.Failure<Guid>(registration.Error);
        }

        if (await patients.ExistsWithDocumentAsync(document.Value, cancellationToken))
        {
            return Result.Failure<Guid>(PatientErrors.DocumentAlreadyRegistered);
        }

        var patient = registration.Value;
        patients.Add(patient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(patient.Id.Value);
    }
}
