# EduConnect 🎓

A university management system built with **Blazor Server** (.NET 10) and **Entity Framework Core**, featuring role-based access for Admins, Faculty, and Students.

> **Course Assignment:** Visual Programming – Assignment 3  
> **Team:** Usman · Daniyal · Rafiullah

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Default Login Credentials](#default-login-credentials)
- [Team Contributions](#team-contributions)
- [Design Principles](#design-principles)

---

## Overview

EduConnect is a full-stack Blazor Server web application that simulates a university portal. It supports three distinct user roles — Admin, Faculty, and Student — each with a dedicated dashboard and set of operations. The database is managed entirely through EF Core migrations with automatic seeding on first run.

---

## Features

### 👤 Admin
- View and manage all students (add, edit, delete, view detail)
- Manage faculty members and course assignments
- Broadcast notifications to all users
- View grade reports across all courses

### 🧑‍🏫 Faculty
- View assigned courses
- Submit and update student grades
- Receive notifications

### 🎓 Student
- Browse and enroll in available courses
- View personal grades and CGPA
- View notifications
- Manage profile

### ⚙️ System
- Automatic database creation and migration on startup
- Seed data pre-loaded (admin, faculty, students, courses, enrollments)
- Real-time notification bell component
- Role-based route guards (`AuthGuard`)
- Pagination, search bar, status badges, confirm dialogs

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core / Blazor Server (.NET 10) |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (via `Microsoft.EntityFrameworkCore.SqlServer`) |
| UI | Razor Components (`.razor`) |
| Auth | Custom session-based auth via `AuthStateService` |
| Patterns | Repository pattern, SOLID principles, TPH inheritance |

---

## Project Structure

```
EduConnect/
├── Components/
│   ├── Pages/
│   │   ├── Admin/          # AdminCourses, AdminFaculty, StudentList, Broadcast, GradeReport
│   │   ├── Faculty/        # FacultyCourses, GradeSubmission
│   │   ├── Student/        # Enroll, StudentGrades
│   │   ├── AdminDashboard.razor
│   │   ├── FacultyDashboard.razor
│   │   ├── StudentDashboard.razor
│   │   ├── Login.razor
│   │   ├── Register.razor
│   │   └── Home.razor
│   ├── Layout/             # MainLayout, NavMenu, ReconnectModal
│   ├── AlertBox.razor
│   ├── AuthGuard.razor
│   ├── NotificationBell.razor
│   ├── Pagination.razor
│   ├── SearchBar.razor
│   └── RoleLayout.razor
├── Data/
│   ├── EduConnectDbContext.cs   # DbContext + model config + seed data
│   └── EfCoreQueryRunner.cs    # LINQ query demonstrations
├── Interfaces/
│   ├── ICourseService.cs
│   ├── IGradeService.cs
│   ├── IStudentService.cs
│   ├── IRepository.cs
│   └── IValidatable.cs
├── Models/
│   ├── Person.cs           # Abstract base (TPH)
│   ├── Student.cs
│   ├── Faculty.cs
│   ├── Admin.cs
│   ├── Course.cs
│   ├── GradeRecord.cs
│   ├── StudentCourse.cs
│   ├── FacultyCourse.cs
│   ├── Notification.cs
│   └── AuthState.cs
├── Services/
│   ├── AuthStateService.cs
│   ├── StudentService.cs
│   ├── CourseService.cs
│   ├── GradeService.cs
│   ├── FacultyService.cs
│   └── NotificationService.cs
├── Exceptions/
│   ├── CourseFullException.cs
│   └── StudentHasActiveEnrollmentsException.cs
├── Migrations/
│   └── 20260605074930_InitialCreate.cs
├── Program.cs
├── appsettings.json
└── EduConnect.csproj
```

---

## Database Schema

The app uses **Table Per Hierarchy (TPH)** inheritance — all user types (`Student`, `Faculty`, `Admin`) share a single `People` table with a `Role` discriminator column.

```
People (TPH)
├── Id (PK, Guid)
├── FullName
├── Email
├── PasswordHash
├── Role         ← discriminator: "Student" | "Faculty" | "Admin"
├── Semester     ← Student only
└── CGPA         ← Student only

Courses
├── Id (PK, Guid)
├── Code, Title, CreditHours
├── MaxCapacity, IsActive

StudentCourses  (junction: Student ↔ Course)
FacultyCourses  (junction: Faculty ↔ Course)

GradeRecords
├── StudentId (FK), CourseId (FK)
└── Score, LetterGrade (computed)

Notifications
├── UserId (FK → People)
└── Message, Type, IsRead, CreatedAt
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.12+) **or** VS Code with C# Dev Kit
- SQL Server (LocalDB, Express, or Developer edition)

### Steps

**1. Clone the repository**

```bash
git clone https://github.com/your-username/EduConnect.git
cd EduConnect
```

**2. Verify the connection string** (in `appsettings.json`)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EduConnectDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> Uses Windows Authentication. No SSMS setup needed — the database is created automatically on first run.

**3. Run the app**

```bash
dotnet run
```

Or press **F5** in Visual Studio.

On first launch, EF Core will automatically:
- Create the `EduConnectDB` database
- Apply the migration (create all tables)
- Insert seed data (admin, faculty, students, courses)

**4. Open in browser**

Navigate to `https://localhost:5001` (or the port shown in the terminal).

---

### Optional: Run LINQ Query Demos

```bash
dotnet run -- --run-queries
```

This runs all EF Core LINQ demonstrations from `EfCoreQueryRunner.cs` and exits without starting the web server.

---

## Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@edu.pk` | `admin123` |
| Faculty (Dr. Ashfaq) | `ashfaq@edu.pk` | `faculty123` |
| Faculty (Dr. Sumera) | `sumera@edu.pk` | `faculty123` |
| Faculty (UbaidUllah) | `ubaid@edu.pk` | `faculty123` |
| Student (Usman) | `usman@student.edu.pk` | `student123` |
| Student (Rafiullah) | `rafiullah@student.edu.pk` | `student123` |
| Student (Daniyal) | `daniyal@student.edu.pk` | `student123` |

---

## Team Contributions

| Member | Responsibilities |
|--------|-----------------|
| **Usman** | Core architecture · All Models & Interfaces · Services (Auth, Student, Course, Grade, Faculty, Notification) · EF Core DbContext & Seed Data · Migrations · Program.cs DI pipeline · Custom Exceptions · EfCoreQueryRunner |
| **Daniyal** | Admin pages (StudentList, StudentAdd, StudentEdit, StudentDetail, AdminCourses, AdminFaculty) · Shared components (AlertBox, CourseCard, StudentCard, Pagination) |
| **Rafiullah** | Faculty pages (FacultyCourses, GradeSubmission) · Student pages (Enroll, StudentGrades) · Dashboards (Admin, Faculty, Student) · Layout components (NavMenu, MainLayout) |

---

## Design Principles

This project applies **SOLID** principles throughout:

- **SRP** — Each service class has a single responsibility (e.g., `AuthStateService` only manages auth state; `GradeService` only handles grades)
- **OCP** — `Person` is an abstract base; new user types can be added without modifying existing code
- **LSP** — `Student`, `Faculty`, and `Admin` are substitutable for `Person` wherever the base type is used
- **ISP** — Interfaces are role-specific (`IStudentService`, `ICourseService`, `IGradeService`) rather than one large interface
- **DIP** — All services are injected via interfaces through ASP.NET Core's built-in DI container

Custom exceptions (`CourseFullException`, `StudentHasActiveEnrollmentsException`) provide semantic error handling instead of generic exceptions.
