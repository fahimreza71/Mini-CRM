using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniCrm.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000001"),
                column: "ConcurrencyStamp",
                value: "5f7eff6a-5a64-4e1c-a0a2-4cf86b468fa5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000002"),
                column: "ConcurrencyStamp",
                value: "a08a3b6f-36aa-47ce-9308-27946768e15c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("a1a1a1a1-0000-0000-0000-000000000003"),
                column: "ConcurrencyStamp",
                value: "abc58ee7-c80e-401e-b583-3b6be0a7b195");
        }
    }
}
