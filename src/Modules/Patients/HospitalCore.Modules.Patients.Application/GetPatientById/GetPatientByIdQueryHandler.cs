using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;
using HospitalCore.SharedKernel;
using HospitalCore.SharedKernel.Messaging;

namespace HospitalCore.Modules.Patients.Application.GetPatientById;

public sealed class GetPatientByIdQueryHandler(IPatientRepository patients)
    : IQueryHandler<GetPatientByIdQuery, PatientResponse>
{
    public async Task<Result<PatientResponse>> Handle(GetPatientByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var id = new PatientId(query.PatientId);
        var patient = await patients.GetByIdAsync(id, cancellationToken);

        return patient is null
            ? Result.Failure<PatientResponse>(PatientErrors.NotFound(id))
            : Result.Success(PatientResponse.FromPatient(patient));
    }
}
