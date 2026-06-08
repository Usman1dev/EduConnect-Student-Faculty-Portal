using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduConnect.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditHours = table.Column<int>(type: "int", nullable: false),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Semester = table.Column<int>(type: "int", nullable: true),
                    CGPA = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacultyCourses",
                columns: table => new
                {
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyCourses", x => new { x.FacultyId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_FacultyCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacultyCourses_People_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradeRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditHours = table.Column<int>(type: "int", nullable: false),
                    Marks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeRecords_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeRecords_People_StudentId",
                        column: x => x.StudentId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_People_UserId",
                        column: x => x.UserId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourses", x => new { x.StudentId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_StudentCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCourses_People_StudentId",
                        column: x => x.StudentId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "CreditHours", "IsActive", "MaxCapacity", "Title" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "CS-101", 3, true, 30, "Introduction to Programming" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "CS-201", 3, true, 30, "Data Structures" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "CS-401", 3, true, 2, "Artificial Intelligence" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "CS-284", 3, true, 25, "Web Engineering" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "SE-301", 3, true, 20, "Software Design Patterns" }
                });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "admin@edu.pk", "System Admin", "admin123", "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "ashfaq@edu.pk", "Dr. Ashfaq", "faculty123", "Faculty" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "sumera@edu.pk", "Dr. Sumera", "faculty123", "Faculty" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "ubaid@edu.pk", "UbaidUllah", "faculty123", "Faculty" }
                });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "Id", "CGPA", "Email", "FullName", "PasswordHash", "Role", "Semester" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555555"), 3.6m, "usman@student.edu.pk", "Usman", "student123", "Student", 3 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), 3.2m, "rafiullah@student.edu.pk", "Rafiullah", "student123", "Student", 2 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), 3.4m, "daniyal@student.edu.pk", "Daniyal", "student123", "Student", 4 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), 2.9m, "fatima@student.edu.pk", "Fatima", "student123", "Student", 1 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), 3.0m, "ali@student.edu.pk", "Ali Hassan", "student123", "Student", 2 }
                });

            migrationBuilder.InsertData(
                table: "FacultyCourses",
                columns: new[] { "CourseId", "FacultyId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("44444444-4444-4444-4444-444444444444") }
                });

            migrationBuilder.InsertData(
                table: "StudentCourses",
                columns: new[] { "CourseId", "StudentId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("77777777-7777-7777-7777-777777777777") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("88888888-8888-8888-8888-888888888888") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacultyCourses_CourseId",
                table: "FacultyCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeRecords_CourseId",
                table: "GradeRecords",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeRecords_StudentId",
                table: "GradeRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_CourseId",
                table: "StudentCourses",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacultyCourses");

            migrationBuilder.DropTable(
                name: "GradeRecords");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "StudentCourses");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
