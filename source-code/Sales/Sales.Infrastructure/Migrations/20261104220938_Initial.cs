using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__Outbox_SalesDbContext",
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
                    table.PrimaryKey("PK___Outbox_SalesDbContext", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sales_Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BuyerAccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BuyerName = table.Column<string>(type: "TEXT", nullable: false),
                    BuyerEmail = table.Column<string>(type: "TEXT", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", nullable: false),
                    PerformanceStartsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoldEndsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaceValueCents = table.Column<long>(type: "INTEGER", nullable: false),
                    BookingFeeCents = table.Column<long>(type: "INTEGER", nullable: false),
                    OrderTotalCents = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sales_Performances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PerformanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales_Performances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sales_OrderLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeName = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfTickets = table.Column<int>(type: "INTEGER", nullable: false),
                    TicketPriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales_OrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_OrderLine_Sales_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Sales_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales_Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderReference = table.Column<string>(type: "TEXT", nullable: false),
                    Result = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Payment_Sales_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Sales_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales_OfferedTicketType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PriceCents = table.Column<long>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    OnSale = table.Column<bool>(type: "INTEGER", nullable: false),
                    SalesPerformanceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales_OfferedTicketType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_OfferedTicketType_Sales_Performances_SalesPerformanceId",
                        column: x => x.SalesPerformanceId,
                        principalTable: "Sales_Performances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_SalesDbContext_DeliveredAt",
                table: "__Outbox_SalesDbContext",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OfferedTicketType_SalesPerformanceId",
                table: "Sales_OfferedTicketType",
                column: "SalesPerformanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OrderLine_OrderId",
                table: "Sales_OrderLine",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Orders_BuyerAccountId",
                table: "Sales_Orders",
                column: "BuyerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Orders_PerformanceId_Status_HoldEndsAt",
                table: "Sales_Orders",
                columns: new[] { "PerformanceId", "Status", "HoldEndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Payment_OrderId",
                table: "Sales_Payment",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Payment_ProviderReference",
                table: "Sales_Payment",
                column: "ProviderReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Performances_EventId",
                table: "Sales_Performances",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Performances_PerformanceId",
                table: "Sales_Performances",
                column: "PerformanceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_SalesDbContext");

            migrationBuilder.DropTable(
                name: "Sales_OfferedTicketType");

            migrationBuilder.DropTable(
                name: "Sales_OrderLine");

            migrationBuilder.DropTable(
                name: "Sales_Payment");

            migrationBuilder.DropTable(
                name: "Sales_Performances");

            migrationBuilder.DropTable(
                name: "Sales_Orders");
        }
    }
}
