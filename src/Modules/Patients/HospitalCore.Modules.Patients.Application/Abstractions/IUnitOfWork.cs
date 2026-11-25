namespace HospitalCore.Modules.Patients.Application.Abstractions;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes atomically.
    /// </summary>
    /// <exception cref="UniqueConstraintViolationException">A uniqueness rule enforced by the data store was violated.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
