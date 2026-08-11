using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSubscriber.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClickedAt",
                table: "CampaignRecipients",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedAt",
                table: "CampaignRecipients",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackedLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    SubscriberId = table.Column<int>(type: "int", nullable: false),
                    OriginalUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkToken = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClickedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedLinks_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackedLinks_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedLinks_CampaignId",
                table: "TrackedLinks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedLinks_LinkToken",
                table: "TrackedLinks",
                column: "LinkToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackedLinks_SubscriberId",
                table: "TrackedLinks",
                column: "SubscriberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedLinks");

            migrationBuilder.DropColumn(
                name: "ClickedAt",
                table: "CampaignRecipients");

            migrationBuilder.DropColumn(
                name: "OpenedAt",
                table: "CampaignRecipients");
        }
    }
}
