using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportAssistant.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAIAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIAnalysisJson",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AIAnalyzedAt",
                table: "Tickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnalysisApproved",
                table: "Tickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIAnalysisJson",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AIAnalyzedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsAnalysisApproved",
                table: "Tickets");
        }
    }
}
