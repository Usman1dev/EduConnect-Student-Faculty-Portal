# EduConnect – EF Core Database Integration with SQL Server (SSMS)
## Step-by-Step Documentation

**Project:** EduConnect – VP Assignment 2  
**Students:** 241797, 241926, 241938  
**Date:** June 5, 2026  
**Framework:** .NET 10.0, Blazor Server, Entity Framework Core 10.0.8  
**Database:** SQL Server (via SSMS) – Database Name: `EduConnectDB`

---

## Table of Contents

1. [Overview](#1-overview)
2. [Step 1 – Install NuGet Packages](#step-1--install-nuget-packages)
3. [Step 2 – Create Junction Models](#step-2--create-junction-models)
4. [Step 3 – Refactor Existing Models](#step-3--refactor-existing-models-for-ef-core)
5. [Step 4 – Create the DbContext](#step-4--create-the-dbcontext)
6. [Step 5 – Add Connection String](#step-5--add-connection-string)
7. [Step 6 – Update Program.cs](#step-6--update-programcs)
8. [Step 7 – Refactor All Services](#step-7--refactor-all-services-to-use-dbcontext)
9. [Step 8 – Delete Old Files](#step-8--delete-old-files)
10. [Step 9 – Update Blazor Components](#step-9--update-blazor-components)
11. [Step 10 – Create & Apply Migration](#step-10--create--apply-ef-core-migration)
12. [Step 11 – Build & Verify](#step-11--build--verify)
13. [Database Schema](#database-schema)
14. [Seed Data](#seed-data)

---

## 1. Overview

The EduConnect Blazor Server application originally used **in-memory static lists** (`SeedData.cs`) with **JSON file persistence** (`DataPersistenceService.cs`). This document describes the complete process of migrating the data layer to **Entity Framework Core** with **SQL Server**, so all data is stored in a relational database that can be managed via **SQL Server Management Studio (SSMS)**.

### What Changed

| Before | After |
|--------|-------|
| Static in-memory lists (`SeedData.cs`) | SQL Server database (`EduConnectDB`) |
| JSON file persistence (`DataPersistenceService.cs`) | EF Core with automatic migrations |
| Direct object references (circular) | Junction tables with proper FK relationships |
| No database schema | Full relational schema with TPH inheritance |

---

## Step 1 – Install NuGet Packages

Three NuGet packages were installed plus the `dotnet-ef` global CLI tool.

### Commands Executed:
```
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

### Updated `EduConnect.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.8" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.8" />
  </ItemGroup>
</Project>
```

### Purpose of Each Package:
- **Microsoft.EntityFrameworkCore.SqlServer** – The SQL Server database provider for EF Core
- **Microsoft.EntityFrameworkCore.Tools** – Enables `Add-Migration` and `Update-Database` commands in Package Manager Console
- **Microsoft.EntityFrameworkCore.Design** – Design-time services for EF Core (required for migrations)
- **dotnet-ef** – Command-line tool for running EF Core commands from terminal

---

## Step 2 – Create Junction Models

The original code used direct object references (e.g., `Student` had a `List<Course> Enrollments`). In a relational database, **many-to-many** relationships require **junction (join) tables**.

### New File: `Models/StudentCourse.cs`
```csharp
using System;

namespace EduConnect.Models
{
    public class StudentCourse
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
```

### New File: `Models/FacultyCourse.cs`
```csharp
using System;

namespace EduConnect.Models
{
    public class FacultyCourse
    {
        public Guid FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
```

**Why junction tables?**
- `StudentCourse` maps the many-to-many enrollment relationship between Students and Courses
- `FacultyCourse` maps the many-to-many assignment relationship between Faculty and Courses
- Each uses a **composite primary key** (`StudentId + CourseId` or `FacultyId + CourseId`)

---

## Step 3 – Refactor Existing Models for EF Core

### 3.1 Person.cs (Base Class)
- Added `[Key]` attribute to the `Id` property for EF Core primary key mapping
- This class uses **TPH (Table Per Hierarchy)** inheritance — all Person subtypes (Student, Faculty, Admin) are stored in a single `People` table with a `Role` discriminator column

### 3.2 Student.cs
- Replaced `List<Course> Enrollments` → `List<StudentCourse> StudentCourses` (junction navigation)
- Added `[NotMapped]` helper property `Enrollments` for backward compatibility with Razor components
- `List<GradeRecord> Grades` remains as a one-to-many EF navigation property

### 3.3 Faculty.cs
- Replaced `List<Course> AssignedCourses` → `List<FacultyCourse> FacultyCourses` (junction navigation)
- Added `[NotMapped]` helper property `AssignedCourses` for backward compatibility

### 3.4 Course.cs
- Replaced `List<Guid> AssignedFacultyIds` → `List<FacultyCourse> FacultyCourses`
- Replaced `List<Student> Enrolled` → `List<StudentCourse> StudentCourses`
- Added `[NotMapped]` helper properties for `AssignedFacultyIds` and `Enrolled`
- Updated computed `EnrollmentStatus` property to use `StudentCourses.Count`
- Added `[Key]` attribute

### 3.5 GradeRecord.cs
- Added `[Key]` attribute
- Added EF navigation properties: `Student? Student` and `Course? Course`

### 3.6 Notification.cs
- Added `[Key]` attribute
- Added EF navigation property: `Person? User`

---

## Step 4 – Create the DbContext

### New File: `Data/EduConnectDbContext.cs`

The `EduConnectDbContext` is the central EF Core class that:
1. Defines all `DbSet<>` properties (one per table)
2. Configures relationships in `OnModelCreating`
3. Seeds initial data

### Key Configurations:

**TPH Discriminator:**
```csharp
modelBuilder.Entity<Person>()
    .HasDiscriminator<string>("Role")
    .HasValue<Student>("Student")
    .HasValue<Faculty>("Faculty")
    .HasValue<Admin>("Admin");
```

**Composite Keys for Junction Tables:**
```csharp
modelBuilder.Entity<StudentCourse>()
    .HasKey(sc => new { sc.StudentId, sc.CourseId });

modelBuilder.Entity<FacultyCourse>()
    .HasKey(fc => new { fc.FacultyId, fc.CourseId });
```

**Delete Behavior:**
All foreign keys use `DeleteBehavior.Restrict` to prevent accidental cascade deletions.

**Seed Data:**
All the original seed data from `SeedData.cs` is now configured in `OnModelCreating` using `HasData()`:
- 1 Admin, 3 Faculty, 5 Students
- 5 Courses
- 5 FacultyCourse assignments
- 7 StudentCourse enrollments

---

## Step 5 – Add Connection String

### Modified File: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EduConnectDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

- **Server=localhost** – Connects to the local SQL Server instance
- **Database=EduConnectDB** – The database name (EF Core creates it automatically)
- **Trusted_Connection=True** – Uses Windows Authentication (no username/password needed)
- **TrustServerCertificate=True** – Bypasses SSL certificate validation for local development

---

## Step 6 – Update Program.cs

### Key Changes:

1. **Added EF Core DbContext registration:**
```csharp
builder.Services.AddDbContext<EduConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

2. **Removed** `DataPersistenceService` registration

3. **Replaced** `dataService.LoadData()` with `db.Database.Migrate()`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EduConnectDbContext>();
    db.Database.Migrate();
}
```

This ensures the database is automatically created and all migrations are applied when the app starts.

---

## Step 7 – Refactor All Services to Use DbContext

Every service was updated to inject `EduConnectDbContext` instead of `DataPersistenceService`.

### Changes Per Service:

| Service | Old Dependency | New Dependency | Key Changes |
|---------|---------------|----------------|-------------|
| `StudentService` | `DataPersistenceService` + `SeedData.Students` | `EduConnectDbContext` | Uses `_context.Students` with `Include()` for eager loading |
| `CourseService` | `DataPersistenceService` + `SeedData.Courses` | `EduConnectDbContext` | Enrollment uses `StudentCourse` junction table |
| `GradeService` | `DataPersistenceService` + `SeedData.Grades` | `EduConnectDbContext` | Uses `_context.GradeRecords` |
| `FacultyService` | `DataPersistenceService` + `SeedData.Faculty` | `EduConnectDbContext` | Uses `_context.FacultyMembers` with `Include()` |
| `AuthStateService` | `SeedData.Users` | `EduConnectDbContext` | Uses `_context.People` for login queries |
| `NotificationService` | `DataPersistenceService` + `SeedData.Notifications` | `EduConnectDbContext` | Uses `_context.Notifications` |

### Common Pattern:
```csharp
// Before:
private readonly List<Student> _students = SeedData.Students;
_persistence.SaveData();

// After:
private readonly EduConnectDbContext _context;
_context.SaveChanges();
```

### Eager Loading Example:
```csharp
public List<Student> GetAll() => _context.Students
    .Include(s => s.StudentCourses).ThenInclude(sc => sc.Course)
    .Include(s => s.Grades)
    .ToList();
```

---

## Step 8 – Delete Old Files

The following files were **deleted** as they are no longer needed:

| File | Reason |
|------|--------|
| `Services/DataPersistenceService.cs` | Replaced by EF Core `SaveChanges()` |
| `Services/SeedData.cs` | Seed data moved to `DbContext.OnModelCreating()` |

---

## Step 9 – Update Blazor Components

Four Razor components had direct references to `SeedData.*` that needed to be replaced with `DbContext` queries:

### Components Updated:

1. **`RoleLayout.razor`** – Injected `EduConnectDbContext`, replaced `SeedData.Students.Count` → `DbContext.Students.Count()`, `SeedData.Courses.Count` → `DbContext.Courses.Count()`

2. **`Pages/Admin/Broadcast.razor`** – Replaced `SeedData.Users` → `DbContext.People.ToList()`

3. **`Pages/Admin/GradeReport.razor`** – Replaced all `SeedData.Students` references with a local `_students` list loaded from `DbContext.Students.Include(s => s.Grades)` in `OnInitialized()`

4. **`Pages/Home.razor`** – Replaced `SeedData.Students.Count` and `SeedData.Courses.Count` → `DbContext.Students.Count()` and `DbContext.Courses.Count()`

---

## Step 10 – Create & Apply EF Core Migration

### Commands Executed:

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### What Happened:
1. `dotnet ef migrations add InitialCreate` – Generated migration files in the `Migrations/` folder
2. `dotnet ef database update` – Connected to SQL Server, created the `EduConnectDB` database, created all tables, inserted seed data

### Tables Created:
- `People` (with TPH discriminator column `Role`)
- `Courses`
- `GradeRecords`
- `Notifications`
- `StudentCourses` (junction table)
- `FacultyCourses` (junction table)
- `__EFMigrationsHistory` (EF Core internal tracking)

---

## Step 11 – Build & Verify

```
dotnet build
```

**Result:** ✅ Build succeeded – 0 Warning(s), 0 Error(s)

The database was successfully created in SQL Server. You can verify by:
1. Open **SQL Server Management Studio (SSMS)**
2. Connect to `localhost`
3. Expand **Databases** → find **EduConnectDB**
4. Expand **Tables** → you will see: `People`, `Courses`, `GradeRecords`, `Notifications`, `StudentCourses`, `FacultyCourses`
5. Right-click any table → **Select Top 1000 Rows** to see the seed data

---

## Database Schema

### Entity Relationship Diagram:

```
┌─────────────────────────────────────────────────┐
│                    People (TPH)                  │
│─────────────────────────────────────────────────│
│ Id (PK, uniqueidentifier)                        │
│ FullName (nvarchar)                              │
│ Email (nvarchar)                                 │
│ PasswordHash (nvarchar)                          │
│ Role (nvarchar) ← Discriminator                  │
│ Semester (int) ← Student only                    │
│ CGPA (decimal) ← Student only                    │
└──────────┬──────────────────┬───────────────────┘
           │                  │
           │ 1:N              │ 1:N
           ▼                  ▼
┌──────────────────┐  ┌───────────────────┐
│   StudentCourses │  │  FacultyCourses   │
│──────────────────│  │───────────────────│
│ StudentId (PK,FK)│  │ FacultyId (PK,FK) │
│ CourseId (PK,FK) │  │ CourseId (PK,FK)  │
└────────┬─────────┘  └────────┬──────────┘
         │                     │
         │ N:1                 │ N:1
         ▼                     ▼
┌────────────────────────────────────┐
│              Courses               │
│────────────────────────────────────│
│ Id (PK, uniqueidentifier)          │
│ Code (nvarchar)                    │
│ Title (nvarchar)                   │
│ CreditHours (int)                  │
│ MaxCapacity (int)                  │
│ IsActive (bit)                     │
└──────────┬─────────────────────────┘
           │
           │ 1:N
           ▼
┌────────────────────────────────────┐
│           GradeRecords             │
│────────────────────────────────────│
│ Id (PK, uniqueidentifier)          │
│ StudentId (FK → People)            │
│ CourseId (FK → Courses)            │
│ CourseTitle (nvarchar)             │
│ CreditHours (int)                  │
│ Marks (int)                        │
└────────────────────────────────────┘

┌────────────────────────────────────┐
│          Notifications             │
│────────────────────────────────────│
│ Id (PK, uniqueidentifier)          │
│ Message (nvarchar)                 │
│ NotificationType (int)             │
│ UserId (FK → People)               │
│ IsRead (bit)                       │
│ Timestamp (datetime2)              │
└────────────────────────────────────┘
```

---

## Seed Data

### People Table:

| Id | FullName | Email | Role |
|----|----------|-------|------|
| 11111111-... | System Admin | admin@edu.pk | Admin |
| 22222222-... | Dr. Ashfaq | ashfaq@edu.pk | Faculty |
| 33333333-... | Dr. Sumera | sumera@edu.pk | Faculty |
| 44444444-... | UbaidUllah | ubaid@edu.pk | Faculty |
| 55555555-... | Usman | usman@student.edu.pk | Student |
| 66666666-... | Rafiullah | rafiullah@student.edu.pk | Student |
| 77777777-... | Daniyal | daniyal@student.edu.pk | Student |
| 88888888-... | Fatima | fatima@student.edu.pk | Student |
| 99999999-... | Ali Hassan | ali@student.edu.pk | Student |

### Courses Table:

| Code | Title | Credits | Capacity |
|------|-------|---------|----------|
| CS-101 | Introduction to Programming | 3 | 30 |
| CS-201 | Data Structures | 3 | 30 |
| CS-401 | Artificial Intelligence | 3 | 2 |
| CS-284 | Web Engineering | 3 | 25 |
| SE-301 | Software Design Patterns | 3 | 20 |

### Login Credentials:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@edu.pk | admin123 |
| Faculty | ashfaq@edu.pk | faculty123 |
| Faculty | sumera@edu.pk | faculty123 |
| Faculty | ubaid@edu.pk | faculty123 |
| Student | usman@student.edu.pk | student123 |
| Student | rafiullah@student.edu.pk | student123 |

---

## OOP Principles Maintained

Even after the EF Core migration, all SOLID principles are still applied:

- **SRP (Single Responsibility):** Each service handles only one concern (StudentService → students, CourseService → courses, etc.)
- **OCP (Open/Closed):** IRepository interface allows extension without modifying existing code
- **LSP (Liskov Substitution):** Student/Faculty/Admin can substitute for Person without breaking behavior
- **ISP (Interface Segregation):** Separate interfaces (IStudentService, ICourseService, IGradeService) prevent forced implementation of unused methods
- **DIP (Dependency Inversion):** All services depend on abstractions (interfaces) injected via DI container, not concrete classes

---

*End of Document*

