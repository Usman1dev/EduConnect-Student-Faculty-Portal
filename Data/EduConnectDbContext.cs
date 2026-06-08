using System;
using Microsoft.EntityFrameworkCore;
using EduConnect.Models;

namespace EduConnect.Data
{
    public class EduConnectDbContext : DbContext
    {
        public EduConnectDbContext(DbContextOptions<EduConnectDbContext> options)
            : base(options)
        {
        }

        // ── DbSets ───────────────────────────────────────────────
        public DbSet<Person> People { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> FacultyMembers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<GradeRecord> GradeRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<FacultyCourse> FacultyCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── TPH (Table Per Hierarchy) for Person ─────────────
            modelBuilder.Entity<Person>()
                .HasDiscriminator<string>("Role")
                .HasValue<Student>("Student")
                .HasValue<Faculty>("Faculty")
                .HasValue<Admin>("Admin");

            modelBuilder.Entity<Person>().ToTable("People");

            // ── StudentCourse junction table (composite key) ─────
            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.StudentId, sc.CourseId });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── FacultyCourse junction table (composite key) ─────
            modelBuilder.Entity<FacultyCourse>()
                .HasKey(fc => new { fc.FacultyId, fc.CourseId });

            modelBuilder.Entity<FacultyCourse>()
                .HasOne(fc => fc.Faculty)
                .WithMany(f => f.FacultyCourses)
                .HasForeignKey(fc => fc.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FacultyCourse>()
                .HasOne(fc => fc.Course)
                .WithMany(c => c.FacultyCourses)
                .HasForeignKey(fc => fc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── GradeRecord relationships ────────────────────────
            modelBuilder.Entity<GradeRecord>()
                .HasOne(g => g.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GradeRecord>()
                .HasOne(g => g.Course)
                .WithMany(c => c.GradeRecords)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Notification relationship ────────────────────────
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Ignore computed properties ───────────────────────
            modelBuilder.Entity<GradeRecord>().Ignore(g => g.LetterGrade);
            modelBuilder.Entity<GradeRecord>().Ignore(g => g.GradePoints);
            modelBuilder.Entity<Course>().Ignore(c => c.EnrollmentStatus);
            modelBuilder.Entity<Course>().Ignore(c => c.AssignedFacultyIds);
            modelBuilder.Entity<Course>().Ignore(c => c.Enrolled);
            modelBuilder.Entity<Student>().Ignore(s => s.Enrollments);
            modelBuilder.Entity<Faculty>().Ignore(f => f.AssignedCourses);

            // ── CGPA precision ───────────────────────────────────
            modelBuilder.Entity<Student>()
                .Property(s => s.CGPA)
                .HasPrecision(3, 2);

            // ══════════════════════════════════════════════════════
            // ██  SEED DATA  ██
            // ══════════════════════════════════════════════════════

            // ── Admin ────────────────────────────────────────────
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = adminId,
                FullName = "System Admin",
                Email = "admin@edu.pk",
                PasswordHash = "admin123"
            });

            // ── Faculty ──────────────────────────────────────────
            var ashfaqId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sumeraId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var ubaidId  = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Faculty>().HasData(
                new { Id = ashfaqId, FullName = "Dr. Ashfaq", Email = "ashfaq@edu.pk", PasswordHash = "faculty123" },
                new { Id = sumeraId, FullName = "Dr. Sumera", Email = "sumera@edu.pk", PasswordHash = "faculty123" },
                new { Id = ubaidId,  FullName = "UbaidUllah", Email = "ubaid@edu.pk",  PasswordHash = "faculty123" }
            );

            // ── Students ─────────────────────────────────────────
            var usmanId     = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var rafiullahId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var daniyalId   = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var fatimaId    = Guid.Parse("88888888-8888-8888-8888-888888888888");
            var aliId       = Guid.Parse("99999999-9999-9999-9999-999999999999");

            modelBuilder.Entity<Student>().HasData(
                new { Id = usmanId,     FullName = "Usman",      Email = "usman@student.edu.pk",      PasswordHash = "student123", Semester = 3, CGPA = 3.6m },
                new { Id = rafiullahId, FullName = "Rafiullah",  Email = "rafiullah@student.edu.pk",  PasswordHash = "student123", Semester = 2, CGPA = 3.2m },
                new { Id = daniyalId,   FullName = "Daniyal",    Email = "daniyal@student.edu.pk",    PasswordHash = "student123", Semester = 4, CGPA = 3.4m },
                new { Id = fatimaId,    FullName = "Fatima",     Email = "fatima@student.edu.pk",     PasswordHash = "student123", Semester = 1, CGPA = 2.9m },
                new { Id = aliId,       FullName = "Ali Hassan", Email = "ali@student.edu.pk",        PasswordHash = "student123", Semester = 2, CGPA = 3.0m }
            );

            // ── Courses ──────────────────────────────────────────
            var cs101Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var cs201Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var cs401Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var cs284Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var se301Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

            modelBuilder.Entity<Course>().HasData(
                new { Id = cs101Id, Code = "CS-101", Title = "Introduction to Programming", CreditHours = 3, MaxCapacity = 30, IsActive = true },
                new { Id = cs201Id, Code = "CS-201", Title = "Data Structures",             CreditHours = 3, MaxCapacity = 30, IsActive = true },
                new { Id = cs401Id, Code = "CS-401", Title = "Artificial Intelligence",     CreditHours = 3, MaxCapacity = 2,  IsActive = true },
                new { Id = cs284Id, Code = "CS-284", Title = "Web Engineering",             CreditHours = 3, MaxCapacity = 25, IsActive = true },
                new { Id = se301Id, Code = "SE-301", Title = "Software Design Patterns",    CreditHours = 3, MaxCapacity = 20, IsActive = true }
            );

            // ── FacultyCourse assignments ────────────────────────
            modelBuilder.Entity<FacultyCourse>().HasData(
                new { FacultyId = ashfaqId, CourseId = cs101Id },
                new { FacultyId = ashfaqId, CourseId = cs201Id },
                new { FacultyId = ashfaqId, CourseId = cs401Id },
                new { FacultyId = sumeraId, CourseId = cs284Id },
                new { FacultyId = ubaidId,  CourseId = se301Id }
            );

            // ── StudentCourse enrollments ────────────────────────
            modelBuilder.Entity<StudentCourse>().HasData(
                new { StudentId = usmanId,     CourseId = cs101Id },
                new { StudentId = usmanId,     CourseId = cs201Id },
                new { StudentId = usmanId,     CourseId = cs401Id },
                new { StudentId = rafiullahId, CourseId = cs101Id },
                new { StudentId = rafiullahId, CourseId = cs401Id },
                new { StudentId = daniyalId,   CourseId = cs101Id },
                new { StudentId = fatimaId,    CourseId = cs201Id }
            );
        }
    }
}
