using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admission.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__Outbox_AdmissionDbContext",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK___Outbox_AdmissionDbContext", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admission_Performances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", nullable: false),
                    VenueName = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DoorsTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admission_Performances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admission_TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamRole = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admission_TeamMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admission_Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeName = table.Column<string>(type: "TEXT", nullable: false),
                    BuyerAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BuyersName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admission_Tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_AdmissionDbContext_DeliveredAt",
                table: "__Outbox_AdmissionDbContext",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Performances_EventId",
                table: "Admission_Performances",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Performances_PerformanceId",
                table: "Admission_Performances",
                column: "PerformanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admission_TeamMembers_OrganizerId_AccountId",
                table: "Admission_TeamMembers",
                columns: new[] { "OrganizerId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Tickets_BuyerAccountId",
                table: "Admission_Tickets",
                column: "BuyerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Tickets_OrderId",
                table: "Admission_Tickets",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Tickets_PerformanceId",
                table: "Admission_Tickets",
                column: "PerformanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Admission_Tickets_TicketCode",
                table: "Admission_Tickets",
                column: "TicketCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_AdmissionDbContext");

            migrationBuilder.DropTable(
                name: "Admission_Performances");

            migrationBuilder.DropTable(
                name: "Admission_TeamMembers");

            migrationBuilder.DropTable(
                name: "Admission_Tickets");
        }
    }
}
