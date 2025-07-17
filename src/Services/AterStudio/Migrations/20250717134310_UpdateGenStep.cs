using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AterStudio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGenStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenActionTpls");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "GenSteps");

            migrationBuilder.DropColumn(
                name: "GenStepType",
                table: "GenSteps");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "GenSteps",
                newName: "TemplatePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplatePath",
                table: "GenSteps",
                newName: "Path");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "GenSteps",
                type: "TEXT",
                maxLength: 100000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenStepType",
                table: "GenSteps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GenActionTpls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionContent = table.Column<string>(type: "TEXT", maxLength: 10001024, nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenActionTpls", x => x.Id);
                });
        }
    }
}
