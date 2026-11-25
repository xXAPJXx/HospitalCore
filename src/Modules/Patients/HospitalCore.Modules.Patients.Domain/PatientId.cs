namespace HospitalCore.Modules.Patients.Domain;

public readonly record struct PatientId(Guid Value)
{
    public static PatientId New() => new(Guid.CreateVersion7());
}
