using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Handles grade submission and CGPA calculation only.
    /// DIP: Receives NotificationService, ICourseService, and EduConnectDbContext dependencies through DI.
    /// ISP: Only implements grade-related methods via IGradeService.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly NotificationService _notifications;
        private readonly ICourseService _courses;
        private readonly EduConnectDbContext _context;

        public GradeService(NotificationService notifications, ICourseService courses, EduConnectDbContext context)
        {
            _notifications = notifications;
            _courses = courses;
            _context = context;
        }

        public void SubmitGrade(GradeRecord grade)
        {
            if (grade.Marks < 0 || grade.Marks > 100) throw new ArgumentException("Marks must be between 0 and 100.");
            if (grade.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (string.IsNullOrWhiteSpace(grade.CourseTitle)) throw new ArgumentException("Course title is required.");

            var existing = _context.GradeRecords.FirstOrDefault(g => g.StudentId == grade.StudentId && g.CourseId == grade.CourseId);
            if (existing != null)
            {
                existing.Marks = grade.Marks;
                existing.CourseTitle = grade.CourseTitle;
                existing.CreditHours = grade.CreditHours;
            }
            else
            {
                if (grade.Id == Guid.Empty) grade.Id = Guid.NewGuid();
                _context.GradeRecords.Add(grade);
            }

            _context.SaveChanges();

            // Recalculate CGPA
            var student = _context.Students.FirstOrDefault(s => s.Id == grade.StudentId);
            if (student != null)
            {
                student.CGPA = ComputeCGPA(grade.StudentId);
                _context.SaveChanges();
                _notifications.AddNotification($"A grade was posted for {grade.CourseTitle}: {grade.LetterGrade} ({grade.Marks}).", NotificationType.GradePosted, student.Id);
            }
        }

        public List<GradeRecord> GetGradesForCourse(Guid courseId) => _context.GradeRecords
            .Where(g => g.CourseId == courseId)
            .OrderBy(g => g.CourseTitle)
            .ToList();

        public List<GradeRecord> GetGradesForStudent(Guid studentId) => _context.GradeRecords
            .Where(g => g.StudentId == studentId)
            .OrderBy(g => g.CourseTitle)
            .ToList();

        public decimal ComputeCGPA(Guid studentId)
        {
            var studentGrades = GetGradesForStudent(studentId);
            var totalCredits = studentGrades.Sum(g => g.CreditHours);
            if (totalCredits == 0) return 0.0m;
            var weighted = studentGrades.Sum(g => g.GradePoints * g.CreditHours) / totalCredits;
            return Math.Round(weighted, 2);
        }
    }
}
