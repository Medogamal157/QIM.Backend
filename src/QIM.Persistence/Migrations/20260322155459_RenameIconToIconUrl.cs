using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QIM.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameIconToIconUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "Categories",
                newName: "IconUrl");

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "IconUrl",
                table: "Categories",
                newName: "Icon");
        }
    }
}
