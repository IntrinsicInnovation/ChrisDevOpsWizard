using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsWizard.Migrations
{
    /// <inheritdoc />
    public partial class devopswizard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Pipelines",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Pipelines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Pipelines");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Pipelines");
        }
    }
}
