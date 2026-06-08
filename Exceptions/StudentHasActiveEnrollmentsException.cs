using System;

namespace EduConnect.Exceptions
{
    public class StudentHasActiveEnrollmentsException : Exception
    {
        public StudentHasActiveEnrollmentsException(string message) : base(message) { }
    }
}