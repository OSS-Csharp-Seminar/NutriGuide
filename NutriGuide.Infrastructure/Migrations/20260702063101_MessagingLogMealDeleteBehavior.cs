using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriGuide.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MessagingLogMealDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessagingLogs_MealLogs_MealLogId",
                table: "MessagingLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_MessagingLogs_MealLogs_MealLogId",
                table: "MessagingLogs",
                column: "MealLogId",
                principalTable: "MealLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessagingLogs_MealLogs_MealLogId",
                table: "MessagingLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_MessagingLogs_MealLogs_MealLogId",
                table: "MessagingLogs",
                column: "MealLogId",
                principalTable: "MealLogs",
                principalColumn: "Id");
        }
    }
}
