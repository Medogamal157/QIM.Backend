using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QIM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PhaseF_BilingualDescription_Keywords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Businesses",
                newName: "DescriptionEn");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Businesses",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessKeywords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessId = table.Column<int>(type: "integer", nullable: false),
                    Keyword = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessKeywords_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessKeywords_BusinessId",
                table: "BusinessKeywords",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessKeywords");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "Businesses",
                newName: "Description");
        }
    }
}
