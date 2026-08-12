using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSubscriber.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsubscribeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeToken",
                table: "Subscribers",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE Subscribers SET UnsubscribeToken = REPLACE(UUID(), '-', '') WHERE UnsubscribeToken IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_UnsubscribeToken",
                table: "Subscribers",
                column: "UnsubscribeToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscribers_UnsubscribeToken",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "Subscribers");
        }
    }
}
