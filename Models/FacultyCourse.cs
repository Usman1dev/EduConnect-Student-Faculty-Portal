using System;

namespace EduConnect.Models
{
    /// <summary>
    /// Junction entity for the many-to-many relationship between Faculty and Course (assignment).
    /// </summary>
    public class FacultyCourse
    {
        public Guid FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
