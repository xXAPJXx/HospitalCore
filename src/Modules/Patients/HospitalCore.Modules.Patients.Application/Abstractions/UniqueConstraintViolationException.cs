namespace HospitalCore.Modules.Patients.Application.Abstractions;

/// <summary>
/// Thrown by <see cref="IUnitOfWork"/> when saving would break a uniqueness rule enforced by the data store,
/// for example when two concurrent requests register the same identity document.
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException()
    {
    }

    public UniqueConstraintViolationException(string message)
        : base(message)
    {
    }

    public UniqueConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
