using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EduConnect.Models;

namespace EduConnect.Data
{
    /// <summary>
    /// Replaces SSMS_Queries.sql with pure EF Core LINQ queries.
    /// All 14 verification queries are executed via DbContext — no raw SQL needed.
    /// Run with: dotnet run -- --run-queries
    /// </summary>
    public static class EfCoreQueryRunner
    {
        public static void RunAllQueries(EduConnectDbContext db)
        {
            PrintHeader("EduConnect – EF Core Query Runner (LINQ Replacement for SSMS_Queries.sql)");

            // ── Query 1: List All Tables ──────────────────────────
            PrintSection("1. ALL TABLES IN THE DATABASE");
            var tableNames = db.Model.GetEntityTypes()
                .Select(e => e.GetTableName())
                .Where(t => t != null)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            PrintTable(new[] { "TableName" }, tableNames.Select(t => new[] { t! }).ToList());

            // ── Query 2: All People (Admin, Faculty, Students – TPH) ──
            PrintSection("2. ALL PEOPLE (Admin, Faculty, Students – TPH)");
            var allPeople = db.People
                .ToList()
                .OrderBy(p => p.GetRole())
                .ThenBy(p => p.FullName)
                .ToList();
            PrintTable(
                new[] { "Id", "FullName", "Email", "Role", "Semester", "CGPA" },
                allPeople.Select(p => new[]
                {
                    p.Id.ToString()[..8] + "...",
                    p.FullName,
                    p.Email,
                    p.GetRole(),
                    p is Student s1 ? s1.Semester.ToString() : "-",
                    p is Student s2 ? s2.CGPA.ToString("0.00") : "-"
                }).ToList());

            // ── Query 3: Students Only ────────────────────────────
            PrintSection("3. STUDENTS ONLY");
            var students = db.Students
                .OrderByDescending(s => s.CGPA)
                .ToList();
            PrintTable(
                new[] { "Id", "FullName", "Email", "Semester", "CGPA" },
                students.Select(s => new[]
                {
                    s.Id.ToString()[..8] + "...",
                    s.FullName,
                    s.Email,
                    s.Semester.ToString(),
                    s.CGPA.ToString("0.00")
                }).ToList());

            // ── Query 4: Faculty Only ─────────────────────────────
            PrintSection("4. FACULTY ONLY");
            var faculty = db.FacultyMembers
                .OrderBy(f => f.FullName)
                .ToList();
            PrintTable(
                new[] { "Id", "FullName", "Email" },
                faculty.Select(f => new[]
                {
                    f.Id.ToString()[..8] + "...",
                    f.FullName,
                    f.Email
                }).ToList());

            // ── Query 5: All Courses ──────────────────────────────
            PrintSection("5. ALL COURSES");
            var courses = db.Courses
                .OrderBy(c => c.Code)
                .ToList();
            PrintTable(
                new[] { "Id", "Code", "Title", "Credits", "MaxCapacity", "IsActive" },
                courses.Select(c => new[]
                {
                    c.Id.ToString()[..8] + "...",
                    c.Code,
                    c.Title,
                    c.CreditHours.ToString(),
                    c.MaxCapacity.ToString(),
                    c.IsActive ? "Yes" : "No"
                }).ToList());

            // ── Query 6: Student Enrollments (Many-to-Many Join) ──
            PrintSection("6. STUDENT ENROLLMENTS (StudentCourses JOIN People JOIN Courses)");
            var enrollments = db.StudentCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Course)
                .OrderBy(sc => sc.Student.FullName)
                .ThenBy(sc => sc.Course.Code)
                .ToList();
            PrintTable(
                new[] { "StudentName", "CourseCode", "CourseTitle" },
                enrollments.Select(sc => new[]
                {
                    sc.Student.FullName,
                    sc.Course.Code,
                    sc.Course.Title
                }).ToList());

            // ── Query 7: Faculty-Course Assignments (Many-to-Many Join) ──
            PrintSection("7. FACULTY ASSIGNMENTS (FacultyCourses JOIN People JOIN Courses)");
            var assignments = db.FacultyCourses
                .Include(fc => fc.Faculty)
                .Include(fc => fc.Course)
                .OrderBy(fc => fc.Faculty.FullName)
                .ThenBy(fc => fc.Course.Code)
                .ToList();
            PrintTable(
                new[] { "FacultyName", "CourseCode", "CourseTitle" },
                assignments.Select(fc => new[]
                {
                    fc.Faculty.FullName,
                    fc.Course.Code,
                    fc.Course.Title
                }).ToList());

            // ── Query 8: Enrollment Count Per Course (GROUP BY) ───
            PrintSection("8. ENROLLMENT COUNT PER COURSE (GROUP BY)");
            var enrollmentCounts = db.Courses
                .Include(c => c.StudentCourses)
                .OrderBy(c => c.Code)
                .ToList();
            PrintTable(
                new[] { "Code", "Title", "MaxCapacity", "Enrolled", "SeatsRemaining" },
                enrollmentCounts.Select(c => new[]
                {
                    c.Code,
                    c.Title,
                    c.MaxCapacity.ToString(),
                    c.StudentCourses.Count.ToString(),
                    (c.MaxCapacity - c.StudentCourses.Count).ToString()
                }).ToList());

            // ── Query 9: Grade Records (JOIN with People) ─────────
            PrintSection("9. GRADE RECORDS (GradeRecords JOIN People)");
            var grades = db.GradeRecords
                .Include(g => g.Student)
                .OrderBy(g => g.Student!.FullName)
                .ThenBy(g => g.CourseTitle)
                .ToList();
            if (grades.Any())
            {
                PrintTable(
                    new[] { "StudentName", "CourseTitle", "Marks", "LetterGrade", "CreditHours" },
                    grades.Select(g => new[]
                    {
                        g.Student?.FullName ?? "N/A",
                        g.CourseTitle,
                        g.Marks.ToString(),
                        g.LetterGrade,
                        g.CreditHours.ToString()
                    }).ToList());
            }
            else
            {
                Console.WriteLine("  (No grade records yet — grades appear after using the app)");
            }

            // ── Query 10: Notifications (JOIN with People) ────────
            PrintSection("10. NOTIFICATIONS (Notifications JOIN People)");
            var notifications = db.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.Timestamp)
                .ToList();
            if (notifications.Any())
            {
                PrintTable(
                    new[] { "UserName", "Message", "Type", "IsRead", "Timestamp" },
                    notifications.Select(n => new[]
                    {
                        n.User?.FullName ?? "N/A",
                        n.Message.Length > 40 ? n.Message[..40] + "..." : n.Message,
                        n.NotificationType.ToString(),
                        n.IsRead ? "Yes" : "No",
                        n.Timestamp.ToString("yyyy-MM-dd HH:mm")
                    }).ToList());
            }
            else
            {
                Console.WriteLine("  (No notifications yet — notifications appear after using the app)");
            }

            // ── Query 11: Dean's List (CGPA >= 3.5) ──────────────
            PrintSection("11. DEAN'S LIST (CGPA >= 3.5)");
            var deansList = db.Students
                .Where(s => s.CGPA >= 3.5m)
                .OrderByDescending(s => s.CGPA)
                .ToList();
            if (deansList.Any())
            {
                PrintTable(
                    new[] { "FullName", "Email", "CGPA", "Semester" },
                    deansList.Select(s => new[]
                    {
                        s.FullName,
                        s.Email,
                        s.CGPA.ToString("0.00"),
                        s.Semester.ToString()
                    }).ToList());
            }
            else
            {
                Console.WriteLine("  (No students with CGPA >= 3.5)");
            }

            // ── Query 12: At-Risk Students (CGPA < 2.0) ──────────
            PrintSection("12. AT-RISK STUDENTS (CGPA < 2.0)");
            var atRisk = db.Students
                .Where(s => s.CGPA < 2.0m)
                .OrderBy(s => s.CGPA)
                .ToList();
            if (atRisk.Any())
            {
                PrintTable(
                    new[] { "FullName", "Email", "CGPA", "Semester" },
                    atRisk.Select(s => new[]
                    {
                        s.FullName,
                        s.Email,
                        s.CGPA.ToString("0.00"),
                        s.Semester.ToString()
                    }).ToList());
            }
            else
            {
                Console.WriteLine("  (No at-risk students — all CGPA >= 2.0)");
            }

            // ── Query 13: EF Core Migration History ───────────────
            PrintSection("13. EF CORE MIGRATION HISTORY");
            // __EFMigrationsHistory is not a DbSet — use raw query for this one special table
            var migrations = db.Database
                .SqlQueryRaw<MigrationEntry>("SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId")
                .ToList();
            PrintTable(
                new[] { "MigrationId", "ProductVersion" },
                migrations.Select(m => new[] { m.MigrationId, m.ProductVersion }).ToList());

            // ── Summary ───────────────────────────────────────────
            PrintSection("SUMMARY");
            Console.WriteLine($"  Total People     : {db.People.Count()}");
            Console.WriteLine($"    ├─ Students    : {db.Students.Count()}");
            Console.WriteLine($"    ├─ Faculty     : {db.FacultyMembers.Count()}");
            Console.WriteLine($"    └─ Admins      : {db.Admins.Count()}");
            Console.WriteLine($"  Total Courses    : {db.Courses.Count()}");
            Console.WriteLine($"  Total Enrollments: {db.StudentCourses.Count()}");
            Console.WriteLine($"  Total Grades     : {db.GradeRecords.Count()}");
            Console.WriteLine($"  Total Notifs     : {db.Notifications.Count()}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ All 13 EF Core LINQ queries executed successfully!");
            Console.ResetColor();
            Console.WriteLine(new string('═', 60));
        }

        // ── Helper: tiny DTO for migration history ────────────────
        private class MigrationEntry
        {
            public string MigrationId { get; set; } = "";
            public string ProductVersion { get; set; } = "";
        }

        // ── Printing helpers ──────────────────────────────────────
        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('═', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('═', 60));
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ── {title} ──");
            Console.ResetColor();
        }

        private static void PrintTable(string[] headers, List<string[]> rows)
        {
            if (rows.Count == 0) { Console.WriteLine("  (empty)"); return; }

            // Calculate column widths
            var widths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
                widths[i] = headers[i].Length;
            foreach (var row in rows)
                for (int i = 0; i < row.Length && i < widths.Length; i++)
                    widths[i] = Math.Max(widths[i], (row[i] ?? "").Length);

            // Print header
            Console.Write("  ");
            for (int i = 0; i < headers.Length; i++)
                Console.Write(headers[i].PadRight(widths[i] + 2));
            Console.WriteLine();

            // Print separator
            Console.Write("  ");
            for (int i = 0; i < headers.Length; i++)
                Console.Write(new string('─', widths[i]) + "  ");
            Console.WriteLine();

            // Print rows
            foreach (var row in rows)
            {
                Console.Write("  ");
                for (int i = 0; i < headers.Length && i < row.Length; i++)
                    Console.Write((row[i] ?? "").PadRight(widths[i] + 2));
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ({rows.Count} row{(rows.Count == 1 ? "" : "s")})");
            Console.ResetColor();
        }
    }
}
