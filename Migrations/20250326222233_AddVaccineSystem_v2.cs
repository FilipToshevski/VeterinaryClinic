using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinaryClinic.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccineSystem_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PetVaccine_Pets_PetId",
                table: "PetVaccine");

            migrationBuilder.DropForeignKey(
                name: "FK_PetVaccine_Vaccines_VaccineId",
                table: "PetVaccine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PetVaccine",
                table: "PetVaccine");

            migrationBuilder.RenameTable(
                name: "PetVaccine",
                newName: "PetVaccines");

            migrationBuilder.RenameIndex(
                name: "IX_PetVaccine_VaccineId",
                table: "PetVaccines",
                newName: "IX_PetVaccines_VaccineId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vaccines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Vaccines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdministered",
                table: "PetVaccines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PetVaccines",
                table: "PetVaccines",
                columns: new[] { "PetId", "VaccineId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PetVaccines_Pets_PetId",
                table: "PetVaccines",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PetVaccines_Vaccines_VaccineId",
                table: "PetVaccines",
                column: "VaccineId",
                principalTable: "Vaccines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PetVaccines_Pets_PetId",
                table: "PetVaccines");

            migrationBuilder.DropForeignKey(
                name: "FK_PetVaccines_Vaccines_VaccineId",
                table: "PetVaccines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PetVaccines",
                table: "PetVaccines");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Vaccines");

            migrationBuilder.DropColumn(
                name: "DateAdministered",
                table: "PetVaccines");

            migrationBuilder.RenameTable(
                name: "PetVaccines",
                newName: "PetVaccine");

            migrationBuilder.RenameIndex(
                name: "IX_PetVaccines_VaccineId",
                table: "PetVaccine",
                newName: "IX_PetVaccine_VaccineId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vaccines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PetVaccine",
                table: "PetVaccine",
                columns: new[] { "PetId", "VaccineId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PetVaccine_Pets_PetId",
                table: "PetVaccine",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PetVaccine_Vaccines_VaccineId",
                table: "PetVaccine",
                column: "VaccineId",
                principalTable: "Vaccines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
