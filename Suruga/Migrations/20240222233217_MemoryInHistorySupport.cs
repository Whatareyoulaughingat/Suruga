using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suruga.Migrations
{
    /// <inheritdoc />
    public partial class MemoryInHistorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Json",
                table: "Sessions",
                newName: "History");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "History",
                table: "Sessions",
                newName: "Json");
        }
    }
}
