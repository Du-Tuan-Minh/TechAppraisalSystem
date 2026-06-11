using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class listDepartmentIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalDepartmentIdsJson",
                table: "TechnicalDocuments",
                newName: "ExternalDepartmentIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalDepartmentIds",
                table: "TechnicalDocuments",
                newName: "ExternalDepartmentIdsJson");
        }
    }
}
