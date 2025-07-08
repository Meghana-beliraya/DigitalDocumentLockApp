using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDocumentLockCommon.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_activity_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    activity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    activity_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_activity_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_user_activity_log_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_activity_log_user_id",
                table: "user_activity_log",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_activity_log");
        }
    }
}
