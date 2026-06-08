using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public class GradeRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public int CreditHours { get; set; }
        public int Marks { get; set; }

        // EF Core navigation properties
        public Student? Student { get; set; }
        public Course? Course { get; set; }

        public string LetterGrade
        {
            get
            {
                return Marks switch
                {
                    >= 85 => "A",
                    >= 70 => "B",
                    >= 55 => "C",
                    >= 45 => "D",
                    _ => "F"
                };
            }
        }

        public decimal GradePoints
        {
            get
            {
                return LetterGrade switch
                {
                    "A" => 4.0m,
                    "B" => 3.0m,
                    "C" => 2.0m,
                    "D" => 1.0m,
                    _ => 0.0m
                };
            }
        }
    }
}