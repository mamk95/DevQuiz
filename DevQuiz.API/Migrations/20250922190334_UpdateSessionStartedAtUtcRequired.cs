using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevQuiz.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionStartedAtUtcRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Progresses_SessionId_QuestionId",
                table: "Progresses");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_SessionId_QuestionId",
                table: "Progresses",
                columns: new[] { "SessionId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Progresses_SessionId_QuestionId",
                table: "Progresses");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Sessions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_SessionId_QuestionId",
                table: "Progresses",
                columns: new[] { "SessionId", "QuestionId" });
        }
    }
}
