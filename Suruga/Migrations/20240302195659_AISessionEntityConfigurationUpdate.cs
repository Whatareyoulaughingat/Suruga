using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Suruga.Migrations
{
    /// <inheritdoc />
    public partial class AISessionEntityConfigurationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ConfigurationPrompt",
                table: "Sessions",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigurationPrompt",
                table: "Sessions");
        }
    }
}
