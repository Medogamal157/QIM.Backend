using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoriesToActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs pointing to Categories
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Categories_CategoryId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTypes_Categories_CategoryId",
                table: "ServiceTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            // Rename Categories table → Activities
            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Activities");

            // Rename PK
            migrationBuilder.RenameIndex(
                name: "PK_Categories",
                table: "Activities",
                newName: "PK_Activities");

            // Rename ParentCategoryId → ParentActivityId column in Activities
            migrationBuilder.RenameColumn(
                name: "ParentCategoryId",
                table: "Activities",
                newName: "ParentActivityId");

            // Rename self-referencing index
            migrationBuilder.RenameIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Activities",
                newName: "IX_Activities_ParentActivityId");

            // Rename CategoryId → ActivityId in ServiceTypes
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "ServiceTypes",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceTypes_CategoryId",
                table: "ServiceTypes",
                newName: "IX_ServiceTypes_ActivityId");

            // Rename CategoryId → ActivityId in Businesses
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Businesses",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_CategoryId",
                table: "Businesses",
                newName: "IX_Businesses_ActivityId");

            // Re-add FKs with new names
            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ParentActivityId",
                table: "Activities",
                column: "ParentActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Activities_ActivityId",
                table: "Businesses",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTypes_Activities_ActivityId",
                table: "ServiceTypes",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ParentActivityId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Activities_ActivityId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTypes_Activities_ActivityId",
                table: "ServiceTypes");

            // Rename ActivityId → CategoryId in Businesses
            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "Businesses",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_ActivityId",
                table: "Businesses",
                newName: "IX_Businesses_CategoryId");

            // Rename ActivityId → CategoryId in ServiceTypes
            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "ServiceTypes",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceTypes_ActivityId",
                table: "ServiceTypes",
                newName: "IX_ServiceTypes_CategoryId");

            // Rename ParentActivityId → ParentCategoryId
            migrationBuilder.RenameColumn(
                name: "ParentActivityId",
                table: "Activities",
                newName: "ParentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Activities_ParentActivityId",
                table: "Activities",
                newName: "IX_Categories_ParentCategoryId");

            // Rename Activities table → Categories
            migrationBuilder.RenameIndex(
                name: "PK_Activities",
                table: "Activities",
                newName: "PK_Categories");

            migrationBuilder.RenameTable(
                name: "Activities",
                newName: "Categories");

            // Re-add FKs
            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Categories_CategoryId",
                table: "Businesses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTypes_Categories_CategoryId",
                table: "ServiceTypes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
