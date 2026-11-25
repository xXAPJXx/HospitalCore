using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalCore.Modules.Patients.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "patients");

            migrationBuilder.CreateTable(
                name: "Patients",
                schema: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegisteredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GivenName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DocumentType_DocumentNumber",
                schema: "patients",
                table: "Patients",
                columns: new[] { "DocumentType", "DocumentNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients",
                schema: "patients");
        }
    }
}
