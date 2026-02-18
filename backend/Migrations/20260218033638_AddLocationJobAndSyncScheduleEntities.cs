using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace weatherapp.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationJobAndSyncScheduleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationJobs_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationSyncSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextSyncAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecurringJobId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSyncSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationSyncSchedules_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationSyncSchedules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationJobs_LocationId",
                table: "LocationJobs",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSyncSchedules_LocationId",
                table: "LocationSyncSchedules",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSyncSchedules_UserId_LocationId",
                table: "LocationSyncSchedules",
                columns: new[] { "UserId", "LocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationJobs");

            migrationBuilder.DropTable(
                name: "LocationSyncSchedules");
        }
    }
}
