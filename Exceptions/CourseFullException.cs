using System;

namespace EduConnect.Exceptions
{
    public class CourseFullException : Exception
    {
        public CourseFullException(string message) : base(message) { }
    }
}