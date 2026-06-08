using System.Collections.Generic;
using EduConnect.Models;

namespace EduConnect.Interfaces
{
    public interface IStudentService : IRepository<Student>
    {
        List<Student> Search(string term);
    }
}