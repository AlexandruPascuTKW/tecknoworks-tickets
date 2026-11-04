using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__Outbox_AccountsDbContext",
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
                    table.PrimaryKey("PK___Outbox_AccountsDbContext", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    PlatformRole = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    InvitationTokenHash = table.Column<string>(type: "TEXT", nullable: true),
                    InvitationExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_Organizers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_Organizers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts_TeamMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamRole = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts_TeamMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_TeamMember_Accounts_Organizers_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Accounts_Organizers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX___Outbox_AccountsDbContext_DeliveredAt",
                table: "__Outbox_AccountsDbContext",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Accounts_Email",
                table: "Accounts_Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Accounts_InvitationTokenHash",
                table: "Accounts_Accounts",
                column: "InvitationTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TeamMember_OrganizerId_AccountId",
                table: "Accounts_TeamMember",
                columns: new[] { "OrganizerId", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__Outbox_AccountsDbContext");

            migrationBuilder.DropTable(
                name: "Accounts_Accounts");

            migrationBuilder.DropTable(
                name: "Accounts_TeamMember");

            migrationBuilder.DropTable(
                name: "Accounts_Organizers");
        }
    }
}
