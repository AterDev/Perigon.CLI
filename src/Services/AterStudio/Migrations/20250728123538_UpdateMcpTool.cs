using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AterStudio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMcpTool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "McpTools",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_McpTools_ProjectId",
                table: "McpTools",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_McpTools_Projects_ProjectId",
                table: "McpTools",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_McpTools_Projects_ProjectId",
                table: "McpTools");

            migrationBuilder.DropIndex(
                name: "IX_McpTools_ProjectId",
                table: "McpTools");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "McpTools");
        }
    }
}
