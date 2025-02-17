using System;

namespace AppFin_Program.Services
{
    public class UserSessionService
    {
        private int? _currentUserId;
        public bool IsUserLoggedIn => _currentUserId.HasValue;

        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }

        public int GetCurrentUserId()
        {
            if (!IsUserLoggedIn)
            {
                throw new InvalidOperationException("No user is currently logged in.");
            }
            return _currentUserId!.Value;
        }
        public void ClearSession()
        {
            _currentUserId = null;
        }
    }
}
