using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportAssistant.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedByUserId",
                table: "Tickets",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_CreatedByUserId",
                table: "Tickets",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_CreatedByUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedByUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tickets");
        }
    }
}
