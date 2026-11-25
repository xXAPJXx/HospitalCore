using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Domain;
using Microsoft.EntityFrameworkCore;

namespace HospitalCore.Modules.Patients.Infrastructure.Persistence;

public sealed class PatientsDbContext(DbContextOptions<PatientsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public const string Schema = "patients";

    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PatientsDbContext).Assembly);
    }
}
