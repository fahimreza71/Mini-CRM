using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniCrm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000001"),
                column: "ConcurrencyStamp",
                value: "68af2c71-ad77-4226-8387-3a5fb94048d8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000002"),
                column: "ConcurrencyStamp",
                value: "24560d0a-3c32-471e-937a-a91ce0984c71");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000003"),
                column: "ConcurrencyStamp",
                value: "0161c19a-3bb8-4ff3-b624-a2fefe343073");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000001"),
                column: "ConcurrencyStamp",
                value: "cba3e060-329e-4793-bc14-67136ea51e3c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000002"),
                column: "ConcurrencyStamp",
                value: "be78acd9-5c5e-446f-8f43-0695fa21bc96");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000003"),
                column: "ConcurrencyStamp",
                value: "ed79b9c2-20aa-4991-ac6c-31c89e807aed");
        }
    }
}
