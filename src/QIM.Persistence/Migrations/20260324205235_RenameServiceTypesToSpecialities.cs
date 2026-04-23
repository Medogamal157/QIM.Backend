using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QIM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceTypesToSpecialities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_ServiceTypes_ServiceTypeId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTypes_Activities_ActivityId",
                table: "ServiceTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceTypes",
                table: "ServiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTypes_ActivityId",
                table: "ServiceTypes");

            migrationBuilder.RenameTable(
                name: "ServiceTypes",
                newName: "Specialities");

            migrationBuilder.RenameColumn(
                name: "ServiceTypeId",
                table: "Businesses",
                newName: "SpecialityId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_ServiceTypeId",
                table: "Businesses",
                newName: "IX_Businesses_SpecialityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialities",
                table: "Specialities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Specialities_ActivityId",
                table: "Specialities",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialities_Activities_ActivityId",
                table: "Specialities",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Specialities_SpecialityId",
                table: "Businesses",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Specialities_SpecialityId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialities_Activities_ActivityId",
                table: "Specialities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialities",
                table: "Specialities");

            migrationBuilder.DropIndex(
                name: "IX_Specialities_ActivityId",
                table: "Specialities");

            migrationBuilder.RenameTable(
                name: "Specialities",
                newName: "ServiceTypes");

            migrationBuilder.RenameColumn(
                name: "SpecialityId",
                table: "Businesses",
                newName: "ServiceTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_SpecialityId",
                table: "Businesses",
                newName: "IX_Businesses_ServiceTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceTypes",
                table: "ServiceTypes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ActivityId",
                table: "ServiceTypes",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTypes_Activities_ActivityId",
                table: "ServiceTypes",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_ServiceTypes_ServiceTypeId",
                table: "Businesses",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
