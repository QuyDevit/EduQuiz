using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class createdb_03082024_L2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_WorkplaceType_WorkplaceTypeId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "WorkplaceTypeId",
                table: "User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_User_WorkplaceType_WorkplaceTypeId",
                table: "User",
                column: "WorkplaceTypeId",
                principalTable: "WorkplaceType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_WorkplaceType_WorkplaceTypeId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "WorkplaceTypeId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceId",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_WorkplaceType_WorkplaceTypeId",
                table: "User",
                column: "WorkplaceTypeId",
                principalTable: "WorkplaceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
