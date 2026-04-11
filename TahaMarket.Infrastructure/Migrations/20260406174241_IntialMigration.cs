using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OtpCodes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_PhoneNumber",
                table: "OtpCodes",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OtpCodes_PhoneNumber",
                table: "OtpCodes");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "OtpCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ImageUrl", "IsPhoneVerified", "IsVerified", "Name", "PasswordHash", "PhoneNumber", "RefreshToken", "RefreshTokenExpiry", "UserType" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, false, false, "Admin", "$2a$11$9e.ldSEf8MHdxc5C1fP3jumXJ0Z4PUqeuS7jHa8DRIVxFhAN5bCJK", "01141286090", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" });
        }
    }
}
