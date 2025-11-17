using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Semester",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exam_Semester_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subject_Semester_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSubject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScoreStructure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViolationStructure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSubject_Exam_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSubject_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamSubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExaminerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModeratorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Processing"),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submission_ExamSubject_ExamSubjectId",
                        column: x => x.ExamSubjectId,
                        principalTable: "ExamSubject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExaminerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentCode = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: false),
                    SubmissionName = table.Column<string>(type: "nvarchar(105)", maxLength: 105, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ScoreDetails = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GradedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessment_Submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Violation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViolationType = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Violation_Submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_CreatedAt",
                table: "Assessment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_IsActive",
                table: "Assessment",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_SubmissionId",
                table: "Assessment",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_CreatedAt",
                table: "Exam",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_IsActive",
                table: "Exam",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_SemesterId",
                table: "Exam",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubject_CreatedAt",
                table: "ExamSubject",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubject_ExamId",
                table: "ExamSubject",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubject_IsActive",
                table: "ExamSubject",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSubject_SubjectId",
                table: "ExamSubject",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_CreatedAt",
                table: "Semester",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_IsActive",
                table: "Semester",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_CreatedAt",
                table: "Subject",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_IsActive",
                table: "Subject",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_SemesterId",
                table: "Subject",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_CreatedAt",
                table: "Submission",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_ExamSubjectId",
                table: "Submission",
                column: "ExamSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_IsActive",
                table: "Submission",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Violation_CreatedAt",
                table: "Violation",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Violation_IsActive",
                table: "Violation",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Violation_SubmissionId",
                table: "Violation",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessment");

            migrationBuilder.DropTable(
                name: "Violation");

            migrationBuilder.DropTable(
                name: "Submission");

            migrationBuilder.DropTable(
                name: "ExamSubject");

            migrationBuilder.DropTable(
                name: "Exam");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "Semester");
        }
    }
}
