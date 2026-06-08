using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduConnect.Models
{
    public class Faculty : Person
    {
        // EF Core navigation — many-to-many via junction table
        public List<FacultyCourse> FacultyCourses { get; set; } = new();

        // Helper property (not mapped to DB) to keep old AssignedCourses logic accessible
        [NotMapped]
        public List<Course> AssignedCourses
        {
            get => FacultyCourses?.ConvertAll(fc => fc.Course) ?? new();
            set { } // no-op setter kept for backward compatibility
        }

        public override string GetRole() => "Faculty";
    }
}