using System;
using System.Linq;
using EduConnect.Data;
using EduConnect.Models;

namespace EduConnect.Services
{
    /// <summary>
    /// SRP: Maintains the current authenticated user state and broadcasts auth changes to the UI.
    /// </summary>
    public class AuthStateService
    {
        private readonly EduConnectDbContext _context;
        public AuthState State { get; private set; } = new();
        public event Action? OnAuthChanged;

        public AuthStateService(EduConnectDbContext context)
        {
            _context = context;
        }

        public bool Login(string email, string password)
        {
            var user = _context.People.FirstOrDefault(u => u.Email.ToLower() == email.Trim().ToLower() && u.PasswordHash == password);
            if (user == null) return false;
            State.CurrentUser = user;
            OnAuthChanged?.Invoke();
            return true;
        }

        public void Logout()
        {
            State.CurrentUser = null;
            OnAuthChanged?.Invoke();
        }
    }
}
