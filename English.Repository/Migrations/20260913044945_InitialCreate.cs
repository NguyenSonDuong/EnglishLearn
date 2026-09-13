using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    ConfigKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ConfigValue = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.ConfigKey);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BypassedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    IsDeducted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeckId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Term = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Meaning = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    CategoryType = table.Column<int>(type: "INTEGER", nullable: false),
                    ContextTag = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Phonetics = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ExampleSentence = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningMaterials_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningMaterialId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TestType = table.Column<int>(type: "INTEGER", nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    CorrectAnswer = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    OptionsPayload = table.Column<string>(type: "TEXT", nullable: true),
                    AudioLocalPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ImageLocalPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_LearningMaterials_LearningMaterialId",
                        column: x => x.LearningMaterialId,
                        principalTable: "LearningMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "INTEGER", nullable: false),
                    TestedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyHistories_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RepetitionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    EasinessFactor = table.Column<double>(type: "REAL", nullable: false, defaultValue: 2.5),
                    Interval = table.Column<double>(type: "REAL", nullable: false),
                    NextReviewTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastTestedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyRecords_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterials_DeckId",
                table: "LearningMaterials",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LearningMaterialId",
                table: "Questions",
                column: "LearningMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyHistories_QuestionId",
                table: "StudyHistories",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyRecords_QuestionId",
                table: "StudyRecords",
                column: "QuestionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurations");

            migrationBuilder.DropTable(
                name: "EmergencyLogs");

            migrationBuilder.DropTable(
                name: "StudyHistories");

            migrationBuilder.DropTable(
                name: "StudyRecords");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "LearningMaterials");

            migrationBuilder.DropTable(
                name: "Decks");
        }
    }
}
