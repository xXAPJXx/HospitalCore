using HospitalCore.Modules.Patients.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalCore.Modules.Patients.Infrastructure.Persistence;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(patient => patient.Id);
        builder.Property(patient => patient.Id)
            .HasConversion(id => id.Value, value => new PatientId(value))
            .ValueGeneratedNever();

        builder.ComplexProperty(patient => patient.Name, name =>
        {
            name.Property(n => n.GivenName).HasColumnName("GivenName").HasMaxLength(PersonName.MaxLength);
            name.Property(n => n.FamilyName).HasColumnName("FamilyName").HasMaxLength(PersonName.MaxLength);
        });

        // Owned instead of complex type: EF Core 10 cannot index complex type properties,
        // and the document must be unique across patients.
        builder.OwnsOne(patient => patient.Document, document =>
        {
            document.Property(d => d.Type).HasColumnName("DocumentType").HasConversion<string>().HasMaxLength(EnumMaxLength);
            document.Property(d => d.Number).HasColumnName("DocumentNumber").HasMaxLength(IdentityDocument.MaxNumberLength);
            document.HasIndex(d => new { d.Type, d.Number }).IsUnique();
        });
        builder.Navigation(patient => patient.Document).IsRequired();

        builder.Property(patient => patient.Sex).HasConversion<string>().HasMaxLength(EnumMaxLength);
    }
}
