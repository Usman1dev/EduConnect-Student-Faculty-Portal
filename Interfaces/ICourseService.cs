using System;
using System.Collections.Generic;
using EduConnect.Models;

namespace EduConnect.Interfaces
{
    public interface ICourseService : IRepository<Course>
    {
        void EnrollStudent(Guid courseId, Guid studentId);
        void DropCourse(Guid courseId, Guid studentId);
        List<Course> GetForFaculty(Guid facultyId);
        List<Course> GetAvailable();
    }
}