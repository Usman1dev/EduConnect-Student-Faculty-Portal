namespace EduConnect.Models
{
    public class AuthState
    {
        public Person? CurrentUser { get; set; }

        public bool IsAuthenticated => CurrentUser != null;

        public string Role => CurrentUser?.GetRole() ?? "";

        public bool IsAdmin => Role == "Admin";
        public bool IsFaculty => Role == "Faculty";
        public bool IsStudent => Role == "Student";
    }
}