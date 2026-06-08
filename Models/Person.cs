using System;
using System.ComponentModel.DataAnnotations;

namespace EduConnect.Models
{
    public abstract class Person
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public abstract string GetRole();
    }
}