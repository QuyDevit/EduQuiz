using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_tbl_Group_301024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                table: "Group",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "Group",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionType",
                table: "Group",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "SubscriptionType",
                table: "Group");
        }
    }
}
