using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__Outbox_EventsDbContext",
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
                    table.PrimaryKey("PK___Outbox_EventsDbContext", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_EventImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_EventImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ImageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VenueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VenueName = table.Column<string>(type: "TEXT", nullable: false),
                    VenueAddress = table.Column<string>(type: "TEXT", nullable: false),
                    VenueCapacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HasSales = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_Sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_TeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamRole = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_TeamMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_Venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VenueName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_Venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events_Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DoorsTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_Performance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Performance_Events_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events_Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events_TicketType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    PriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    OnSale = table.Column<bool>(type: "INTEGER", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_TicketType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_TicketType_Events_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events_Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events_SaleLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumberOfTickets = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events_SaleLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_SaleLine_Events_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Events_Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_EventsDbContext_DeliveredAt",
                table: "__Outbox_EventsDbContext",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Events_OrganizerId",
                table: "Events_Events",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Performance_EventId",
                table: "Events_Performance",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SaleLine_SaleId",
                table: "Events_SaleLine",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Sales_OrderId",
                table: "Events_Sales",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_Sales_PerformanceId_HasSales",
                table: "Events_Sales",
                columns: new[] { "PerformanceId", "HasSales" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_TeamMembers_OrganizerId_AccountId",
                table: "Events_TeamMembers",
                columns: new[] { "OrganizerId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_TicketType_EventId",
                table: "Events_TicketType",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Venues_OrganizerId",
                table: "Events_Venues",
                column: "OrganizerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_EventsDbContext");

            migrationBuilder.DropTable(
                name: "Events_EventImages");

            migrationBuilder.DropTable(
                name: "Events_Performance");

            migrationBuilder.DropTable(
                name: "Events_SaleLine");

            migrationBuilder.DropTable(
                name: "Events_TeamMembers");

            migrationBuilder.DropTable(
                name: "Events_TicketType");

            migrationBuilder.DropTable(
                name: "Events_Venues");

            migrationBuilder.DropTable(
                name: "Events_Sales");

            migrationBuilder.DropTable(
                name: "Events_Events");
        }
    }
}
