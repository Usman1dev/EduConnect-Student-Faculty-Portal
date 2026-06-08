using System;

namespace EduConnect.Models
{
    /// <summary>
    /// Junction entity for the many-to-many relationship between Student and Course (enrollment).
    /// </summary>
    public class StudentCourse
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
