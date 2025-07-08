using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDocumentLockCommon.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeTypeAndDeleteIndToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commenting out rename and foreign key drop operations
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Users_UserId",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "IsAdmin",
                table: "Users",
                newName: "isAdmin");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "Users",
                newName: "profile_image_url");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Document",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Document",
                newName: "uploaded_at");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Document",
                newName: "file_path");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Document",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "Document",
                newName: "expiry_date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Document",
                newName: "document_id");

            migrationBuilder.RenameIndex(
                name: "IX_Document_UserId",
                table: "Document",
                newName: "IX_Document_user_id");
            */

            // Add only these 3 columns
            migrationBuilder.AddColumn<bool>(
                name: "DeleteInd",
                table: "Document",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Document",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Document",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            /*
            migrationBuilder.AddForeignKey(
                name: "FK_Document_Users_user_id",
                table: "Document",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the 3 columns
            migrationBuilder.DropColumn(
                name: "DeleteInd",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Document");

            // Commenting out reverse renames and foreign key restoration
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Users_user_id",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "isAdmin",
                table: "Users",
                newName: "IsAdmin");

            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "profile_image_url",
                table: "Users",
                newName: "ProfileImageUrl");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Document",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "uploaded_at",
                table: "Document",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "file_path",
                table: "Document",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "Document",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "Document",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "document_id",
                table: "Document",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Document_user_id",
                table: "Document",
                newName: "IX_Document_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Users_UserId",
                table: "Document",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            */
        }
    }
}
