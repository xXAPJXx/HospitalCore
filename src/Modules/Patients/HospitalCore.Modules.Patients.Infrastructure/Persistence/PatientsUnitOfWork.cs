using HospitalCore.Modules.Patients.Application.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HospitalCore.Modules.Patients.Infrastructure.Persistence;

internal sealed class PatientsUnitOfWork(PatientsDbContext context) : IUnitOfWork
{
    // SQL Server error numbers: 2601 = duplicate key in a unique index, 2627 = unique or primary key constraint.
    private const int DuplicateKeyInUniqueIndex = 2601;
    private const int UniqueConstraintViolated = 2627;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: DuplicateKeyInUniqueIndex or UniqueConstraintViolated })
        {
            throw new UniqueConstraintViolationException("Saving changes violated a unique constraint.", exception);
        }
    }
}
