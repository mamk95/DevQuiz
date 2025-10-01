using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevQuiz.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDifficultyToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Sessions",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Sessions");
        }
    }
}
