using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSubscriber.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCodeRequestedAtToSubscribers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Faz 2 — 2.2.1: LastCodeRequestedAt kolonu ekle (DB tabanlı rate-limiting, Seçenek B)
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCodeRequestedAt",
                table: "Subscribers",
                type: "datetime(6)",
                nullable: true);

            // Faz 2 — 2.2.2: Composite index — cooldown sorgusu için (Email + LastCodeRequestedAt)
            migrationBuilder.CreateIndex(
                name: "idx_subscribers_email_lastrequested",
                table: "Subscribers",
                columns: new[] { "Email", "LastCodeRequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_subscribers_email_lastrequested",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "LastCodeRequestedAt",
                table: "Subscribers");
        }
    }
}
