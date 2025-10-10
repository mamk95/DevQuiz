using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevQuiz.API.Migrations
{
    /// <inheritdoc />
    public partial class AllowParticipantMultipleQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_ParticipantId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "Sessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ParticipantId_QuizId",
                table: "Sessions",
                columns: new[] { "ParticipantId", "QuizId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_QuizId",
                table: "Sessions",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Quizzes_QuizId",
                table: "Sessions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Quizzes_QuizId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_ParticipantId_QuizId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_QuizId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "Sessions",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ParticipantId",
                table: "Sessions",
                column: "ParticipantId");

        }
    }
}
