using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NavigationPlatform.JourneyService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartLocation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StartLatitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    StartLongitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalLocation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ArrivalLatitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    ArrivalLongitude = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransportationType = table.Column<int>(type: "integer", nullable: false),
                    RouteDistanceKm = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDailyGoalAchieved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "public_journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccessCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_public_journeys_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_journeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedByUserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    SharedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shared_journeys_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sharing_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", maxLength: 128, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sharing_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sharing_audit_logs_journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_public_journeys_JourneyId",
                table: "public_journeys",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_shared_journeys_JourneyId",
                table: "shared_journeys",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_sharing_audit_logs_JourneyId",
                table: "sharing_audit_logs",
                column: "JourneyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_journeys");

            migrationBuilder.DropTable(
                name: "shared_journeys");

            migrationBuilder.DropTable(
                name: "sharing_audit_logs");

            migrationBuilder.DropTable(
                name: "journeys");
        }
    }
}
