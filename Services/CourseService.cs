using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Exceptions;
using EduConnect.Interfaces;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Manages course catalog and enrollment rules.
    /// DIP: Receives NotificationService and EduConnectDbContext through DI instead of constructing dependencies. Pages depend on ICourseService.
    /// OCP: IRepository allows extension.
    /// ISP: ICourseService adds specific methods on top of IRepository without polluting other interfaces.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly NotificationService _notifications;
        private readonly EduConnectDbContext _context;

        public CourseService(NotificationService notifications, EduConnectDbContext context)
        {
            _notifications = notifications;
            _context = context;
        }

        public List<Course> GetAll() => _context.Courses
            .Include(c => c.StudentCourses).ThenInclude(sc => sc.Student)
            .Include(c => c.FacultyCourses).ThenInclude(fc => fc.Faculty)
            .ToList();

        public Course? GetById(Guid id) => _context.Courses
            .Include(c => c.StudentCourses).ThenInclude(sc => sc.Student)
            .Include(c => c.FacultyCourses).ThenInclude(fc => fc.Faculty)
            .FirstOrDefault(c => c.Id == id);

        public void Add(Course entity)
        {
            Validate(entity);
            if (_context.Courses.Any(c => c.Code.ToLower() == entity.Code.ToLower())) throw new ArgumentException("A course with this code already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            _context.Courses.Add(entity);
            _context.SaveChanges();

            // Sync faculty assignments if AssignedFacultyIds were provided
            SyncFacultyAssignment(entity);
        }

        public void Update(Course entity)
        {
            Validate(entity);
            var existing = GetById(entity.Id) ?? throw new ArgumentException("Course not found.");
            if (_context.Courses.Any(c => c.Id != entity.Id && c.Code.ToLower() == entity.Code.ToLower())) throw new ArgumentException("A course with this code already exists.");
            if (entity.MaxCapacity < existing.StudentCourses.Count) throw new ArgumentException($"Cannot reduce capacity below current enrollment count ({existing.StudentCourses.Count}).");

            // Remove old faculty assignments
            var oldFacultyLinks = _context.FacultyCourses.Where(fc => fc.CourseId == existing.Id).ToList();
            _context.FacultyCourses.RemoveRange(oldFacultyLinks);

            existing.Code = entity.Code;
            existing.Title = entity.Title;
            existing.CreditHours = entity.CreditHours;
            existing.MaxCapacity = entity.MaxCapacity;
            existing.IsActive = entity.IsActive;
            _context.SaveChanges();

            // Add new faculty assignments
            if (entity.FacultyCourses != null && entity.FacultyCourses.Any())
            {
                foreach (var fc in entity.FacultyCourses)
                {
                    if (!_context.FacultyCourses.Any(x => x.FacultyId == fc.FacultyId && x.CourseId == existing.Id))
                    {
                        _context.FacultyCourses.Add(new FacultyCourse { FacultyId = fc.FacultyId, CourseId = existing.Id });
                    }
                }
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var course = _context.Courses
                .Include(c => c.StudentCourses)
                .FirstOrDefault(c => c.Id == id);
            if (course == null) return;
            if (course.StudentCourses.Any()) throw new InvalidOperationException($"Course {course.Code} has enrolled students and cannot be deleted.");

            // Remove faculty assignments
            var facultyLinks = _context.FacultyCourses.Where(fc => fc.CourseId == id).ToList();
            _context.FacultyCourses.RemoveRange(facultyLinks);

            _context.Courses.Remove(course);
            _context.SaveChanges();
        }

        public void EnrollStudent(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive.");
            if (course.EnrollmentStatus == EnrollmentStatus.Full) throw new CourseFullException($"Course {course.Code} is full.");
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            if (_context.StudentCourses.Any(sc => sc.StudentId == studentId && sc.CourseId == courseId)) throw new InvalidOperationException($"{student.FullName} is already enrolled in {course.Code}.");

            _context.StudentCourses.Add(new StudentCourse { StudentId = studentId, CourseId = courseId });
            _context.SaveChanges();

            _notifications.AddNotification($"You enrolled in {course.Code} - {course.Title}.", NotificationType.Enrollment, student.Id);
        }

        public void DropCourse(Guid courseId, Guid studentId)
        {
            var course = GetById(courseId) ?? throw new ArgumentException("Course not found.");
            if (!course.IsActive) throw new InvalidOperationException($"Course {course.Code} is inactive and cannot be dropped.");
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId) ?? throw new ArgumentException("Student not found.");
            var enrollment = _context.StudentCourses.FirstOrDefault(sc => sc.StudentId == studentId && sc.CourseId == courseId);
            if (enrollment == null) throw new InvalidOperationException($"{student.FullName} is not enrolled in {course.Code}.");

            _context.StudentCourses.Remove(enrollment);
            _context.SaveChanges();
        }

        public List<Course> GetForFaculty(Guid facultyId) => _context.Courses
            .Include(c => c.StudentCourses).ThenInclude(sc => sc.Student)
            .Include(c => c.FacultyCourses)
            .Where(c => c.FacultyCourses.Any(fc => fc.FacultyId == facultyId))
            .ToList();

        public List<Course> GetAvailable() => _context.Courses
            .Include(c => c.StudentCourses)
            .Include(c => c.FacultyCourses)
            .Where(c => c.IsActive)
            .ToList()
            .Where(c => c.EnrollmentStatus != EnrollmentStatus.Full)
            .ToList();

        private static void Validate(Course entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Code)) throw new ArgumentException("Course code is required.");
            if (string.IsNullOrWhiteSpace(entity.Title)) throw new ArgumentException("Course title is required.");
            if (entity.CreditHours <= 0) throw new ArgumentException("Credit hours must be positive.");
            if (entity.MaxCapacity <= 0) throw new ArgumentException("Max capacity must be positive.");
        }

        private void SyncFacultyAssignment(Course course)
        {
            if (course.FacultyCourses != null)
            {
                foreach (var fc in course.FacultyCourses)
                {
                    if (!_context.FacultyCourses.Any(x => x.FacultyId == fc.FacultyId && x.CourseId == course.Id))
                    {
                        _context.FacultyCourses.Add(new FacultyCourse { FacultyId = fc.FacultyId, CourseId = course.Id });
                    }
                }
                _context.SaveChanges();
            }
        }
    }
}
