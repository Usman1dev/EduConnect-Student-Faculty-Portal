using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EduConnect.Models
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public int CreditHours { get; set; } = 3;
        public int MaxCapacity { get; set; } = 30;
        public bool IsActive { get; set; } = true;

        // EF Core navigation — many-to-many via junction tables
        public List<FacultyCourse> FacultyCourses { get; set; } = new();
        public List<StudentCourse> StudentCourses { get; set; } = new();

        // EF Core navigation — one-to-many
        public List<GradeRecord> GradeRecords { get; set; } = new();

        // Helper properties (not mapped to DB) for backward compatibility
        [NotMapped]
        public List<Guid> AssignedFacultyIds
        {
            get => FacultyCourses?.Select(fc => fc.FacultyId).ToList() ?? new();
            set { } // no-op setter kept for backward compatibility
        }

        [NotMapped]
        public List<Student> Enrolled
        {
            get => StudentCourses?.ConvertAll(sc => sc.Student) ?? new();
            set { } // no-op setter kept for backward compatibility
        }

        [NotMapped]
        public EnrollmentStatus EnrollmentStatus
        {
            get
            {
                int count = StudentCourses?.Count ?? 0;
                if (count >= MaxCapacity) return EnrollmentStatus.Full;
                double percentage = (double)count / MaxCapacity;
                return percentage >= 0.7 ? EnrollmentStatus.AlmostFull : EnrollmentStatus.Open;
            }
        }
    }
}