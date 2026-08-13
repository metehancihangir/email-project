using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSubscriber.API.Migrations
{
    /// <inheritdoc />
    public partial class FixLastCodeRequestedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<DateTime>(
            //     name: "LastCodeRequestedAt",
            //     table: "Subscribers",
            //     type: "datetime(6)",
            //     nullable: true);

            // migrationBuilder.CreateIndex(
            //     name: "idx_subscribers_email_lastrequested",
            //     table: "Subscribers",
            //     columns: new[] { "Email", "LastCodeRequestedAt" });
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
