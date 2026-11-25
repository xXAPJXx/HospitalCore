using HospitalCore.Modules.Patients.Application.Abstractions;
using HospitalCore.Modules.Patients.Application.GetPatientById;
using HospitalCore.Modules.Patients.Application.RegisterPatient;
using HospitalCore.Modules.Patients.Infrastructure.Persistence;
using HospitalCore.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HospitalCore.Modules.Patients.Infrastructure;

public static class PatientsModule
{
    public const string ConnectionStringName = "HospitalCore";

    public static IServiceCollection AddPatientsModule(this IServiceCollection services)
    {
        services.AddDbContext<PatientsDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"The connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString, sqlServer =>
                sqlServer.MigrationsHistoryTable(HistoryRepository.DefaultTableName, PatientsDbContext.Schema));
        });

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<PatientsDbContext>());

        services.AddScoped<ICommandHandler<RegisterPatientCommand, Guid>, RegisterPatientCommandHandler>();
        services.AddScoped<IQueryHandler<GetPatientByIdQuery, PatientResponse>, GetPatientByIdQueryHandler>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
