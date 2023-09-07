using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsWizard.Migrations
{
    /// <inheritdoc />
    public partial class isvisible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Pipelines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Pipelines");
        }
    }
}
