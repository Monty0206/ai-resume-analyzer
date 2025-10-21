using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeAnalyzer.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtractedText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResumeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AtsScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompletenessScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    KeywordScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FormattingScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    StrengthsSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeaknessesSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalyzedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImpactScore = table.Column<int>(type: "int", nullable: false),
                    ActionSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Example = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConfidenceLevel = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsInDemand = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_Analyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analysis_AnalyzedDate",
                table: "Analyses",
                column: "AnalyzedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Analysis_ResumeId",
                table: "Analyses",
                column: "ResumeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_AnalysisId",
                table: "Recommendations",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_Category",
                table: "Recommendations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendation_Priority",
                table: "Recommendations",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Resume_Status",
                table: "Resumes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Resume_UploadedDate",
                table: "Resumes",
                column: "UploadedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Skill_AnalysisId_Category",
                table: "Skills",
                columns: new[] { "AnalysisId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Skill_IsInDemand",
                table: "Skills",
                column: "IsInDemand");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Resumes");
        }
    }
}
