using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NavigationPlatform.RewardService.Worker.Migrations
{
    /// <inheritdoc />
    public partial class InitalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_goal_achievements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_date = table.Column<DateTime>(type: "date", nullable: false),
                    total_distance_km = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    triggering_journey_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_goal_achievements", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_daily_goal_achievements_date",
                table: "daily_goal_achievements",
                column: "achievement_date");

            migrationBuilder.CreateIndex(
                name: "idx_daily_goal_achievements_user_id",
                table: "daily_goal_achievements",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_achievement_date_unique",
                table: "daily_goal_achievements",
                columns: new[] { "user_id", "achievement_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_goal_achievements");
        }
    }
}
