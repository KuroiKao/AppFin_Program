using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public class AuthenticationService
    {
        private readonly FinAppDataBaseContext _dbContext;

        public AuthenticationService(FinAppDataBaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> AuthenticateAsync(string login, string password)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Login == login);
                if (user != null && user.Password == password)
                {
                    return user;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
