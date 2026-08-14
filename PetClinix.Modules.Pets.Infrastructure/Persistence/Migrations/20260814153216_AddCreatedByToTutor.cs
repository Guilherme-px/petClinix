using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetClinix.Modules.Pets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToTutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Tutors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tutors");
        }
    }
}
