using System;
using System.Collections.Generic;
using EduConnect.Models;

namespace EduConnect.Interfaces
{
    public interface IGradeService
    {
        void SubmitGrade(GradeRecord grade);
        List<GradeRecord> GetGradesForCourse(Guid courseId);
        List<GradeRecord> GetGradesForStudent(Guid studentId);
        decimal ComputeCGPA(Guid studentId);
    }
}