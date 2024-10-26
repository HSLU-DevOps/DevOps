using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class TestData : Migration
    {
        private static readonly Guid task1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid task2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid task3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Todos",
                columns: new[] { "Id", "Title", "Description", "Done", "Created", "Completed" },
                values: new object[,]
                {
                    { task1Id, "Sample Task 1", "Description for Sample Task 1", false, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), null },
                    { task2Id, "Sample Task 2", "Description for Sample Task 2", true, DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-2), DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-1), DateTimeKind.Utc) },
                    { task3Id, "Sample Task 3", "Description for Sample Task 3", false, DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-5), DateTimeKind.Utc), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Todos",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    task1Id,
                    task2Id,
                    task3Id
                });
        }
    }
}