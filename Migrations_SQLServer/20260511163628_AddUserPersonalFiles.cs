using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWOMS_ClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPersonalFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EWO_ChatMessage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EWO_ChatMessage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "EWO_ChatGroup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EWO_UserPersonalFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserPersonalFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_UserPersonalFiles_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserPersonalFiles_UserId",
                table: "EWO_UserPersonalFiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EWO_UserPersonalFiles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EWO_ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EWO_ChatMessage");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "EWO_ChatGroup");
        }
    }
}
