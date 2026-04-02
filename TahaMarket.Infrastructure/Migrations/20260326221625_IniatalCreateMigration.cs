using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IniatalCreateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9a6e1c46-80b5-4548-a8d6-0dfd629cf879"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "PhoneNumber", "RefreshToken", "RefreshTokenExpiry", "UserType" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "krymk9920@gmail.com", "Admin", "$2a$11$9e.ldSEf8MHdxc5C1fP3jumXJ0Z4PUqeuS7jHa8DRIVxFhAN5bCJK", "01141286090", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash", "PhoneNumber", "RefreshToken", "RefreshTokenExpiry", "UserType" },
                values: new object[] { new Guid("9a6e1c46-80b5-4548-a8d6-0dfd629cf879"), "krymk9920@gmail.com", "Admin", "$2a$11$jL49sBZqtlJr5fXcZvIvSe6s19.W3c5IZpp0umbuD6MtMqbu0ovVe", "01141286090", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin" });
        }
    }
}
