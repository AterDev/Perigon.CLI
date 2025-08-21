using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AterStudio.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ValueType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SolutionType = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Config = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiDocInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Path = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiDocInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiDocInfo_Solutions_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EntityPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    OpenApiPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Variables = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenActions_Solutions_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OutputContent = table.Column<string>(type: "TEXT", maxLength: 100000, nullable: true),
                    TemplatePath = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    OutputPath = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenSteps_Solutions_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "McpTools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PromptPath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    TemplatePaths = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McpTools_Solutions_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenActionGenSteps",
                columns: table => new
                {
                    GenActionsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenStepsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenActionGenSteps", x => new { x.GenActionsId, x.GenStepsId });
                    table.ForeignKey(
                        name: "FK_GenActionGenSteps_GenActions_GenActionsId",
                        column: x => x.GenActionsId,
                        principalTable: "GenActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenActionGenSteps_GenSteps_GenStepsId",
                        column: x => x.GenStepsId,
                        principalTable: "GenSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiDocInfo_ProjectId",
                table: "ApiDocInfo",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GenActionGenSteps_GenStepsId",
                table: "GenActionGenSteps",
                column: "GenStepsId");

            migrationBuilder.CreateIndex(
                name: "IX_GenActions_Description",
                table: "GenActions",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_GenActions_Name",
                table: "GenActions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_GenActions_ProjectId",
                table: "GenActions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GenSteps_ProjectId",
                table: "GenSteps",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_McpTools_ProjectId",
                table: "McpTools",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiDocInfo");

            migrationBuilder.DropTable(
                name: "Configs");

            migrationBuilder.DropTable(
                name: "GenActionGenSteps");

            migrationBuilder.DropTable(
                name: "McpTools");

            migrationBuilder.DropTable(
                name: "GenActions");

            migrationBuilder.DropTable(
                name: "GenSteps");

            migrationBuilder.DropTable(
                name: "Solutions");
        }
    }
}
