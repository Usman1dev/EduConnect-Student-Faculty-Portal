using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using EduConnect.Interfaces;

namespace EduConnect.Models
{
    public class Student : Person, IValidatable
    {
        public int Semester { get; set; } = 1;
        public decimal CGPA { get; set; } = 0.0m;

        // EF Core navigation — many-to-many via junction table
        public List<StudentCourse> StudentCourses { get; set; } = new();

        // EF Core navigation — one-to-many
        public List<GradeRecord> Grades { get; set; } = new();

        // Helper property (not mapped to DB) to keep old Enrollments logic accessible
        [NotMapped]
        public List<Course> Enrollments
        {
            get => StudentCourses?.ConvertAll(sc => sc.Course) ?? new();
            set { } // no-op setter kept for backward compatibility during transition
        }

        public override string GetRole() => "Student";

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                errorMessage = "Full name is required.";
                return false;
            }
            if (!Email.Contains("@"))
            {
                errorMessage = "Email must contain '@'.";
                return false;
            }
            if (Semester < 1 || Semester > 8)
            {
                errorMessage = "Semester must be between 1 and 8.";
                return false;
            }
            if (CGPA < 0 || CGPA > 4)
            {
                errorMessage = "CGPA must be between 0.0 and 4.0.";
                return false;
            }
            errorMessage = "";
            return true;
        }
    }
}