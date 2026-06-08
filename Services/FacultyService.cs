using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EduConnect.Data;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Manages faculty records only.
    /// DIP: Injects EduConnectDbContext through DI.
    /// LSP: Faculty entity can be substituted anywhere Person is expected without breaking behavior.
    /// </summary>
    public class FacultyService
    {
        private readonly EduConnectDbContext _context;

        public FacultyService(EduConnectDbContext context) => _context = context;

        public List<Faculty> GetAll() => _context.FacultyMembers
            .Include(f => f.FacultyCourses).ThenInclude(fc => fc.Course)
            .ToList();

        public Faculty? GetById(Guid id) => _context.FacultyMembers
            .Include(f => f.FacultyCourses).ThenInclude(fc => fc.Course)
            .FirstOrDefault(f => f.Id == id);

        public void Add(Faculty entity)
        {
            if (string.IsNullOrWhiteSpace(entity.FullName)) throw new ArgumentException("Full name is required.");
            if (string.IsNullOrWhiteSpace(entity.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            if (_context.FacultyMembers.Any(f => f.Email.ToLower() == entity.Email.ToLower())) throw new ArgumentException("A faculty with this email already exists.");
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();
            _context.FacultyMembers.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Faculty entity)
        {
            if (string.IsNullOrWhiteSpace(entity.FullName)) throw new ArgumentException("Full name is required.");
            if (string.IsNullOrWhiteSpace(entity.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(entity.PasswordHash)) throw new ArgumentException("Password is required.");
            var existing = _context.FacultyMembers.FirstOrDefault(f => f.Id == entity.Id) ?? throw new ArgumentException("Faculty not found.");
            if (_context.FacultyMembers.Any(f => f.Id != entity.Id && f.Email.ToLower() == entity.Email.ToLower())) throw new ArgumentException("A faculty with this email already exists.");
            existing.FullName = entity.FullName;
            existing.Email = entity.Email;
            existing.PasswordHash = entity.PasswordHash;
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var faculty = _context.FacultyMembers.FirstOrDefault(f => f.Id == id);
            if (faculty == null) return;

            // Remove faculty-course assignments
            var facultyLinks = _context.FacultyCourses.Where(fc => fc.FacultyId == id).ToList();
            _context.FacultyCourses.RemoveRange(facultyLinks);

            _context.FacultyMembers.Remove(faculty);
            _context.SaveChanges();
        }

        public List<Faculty> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return GetAll();
            term = term.Trim().ToLowerInvariant();
            return _context.FacultyMembers
                .Include(f => f.FacultyCourses).ThenInclude(fc => fc.Course)
                .Where(f => f.FullName.ToLower().Contains(term) || f.Email.ToLower().Contains(term))
                .ToList();
        }
    }
}
