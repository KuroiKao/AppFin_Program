using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public class AuthenticationService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory) : DbContextService(dbContextFactory)
    {
        public async Task<User?> AuthenticateAsync(string login, string password)
        {
            try
            {
                var user = await DbContext.Users.FirstOrDefaultAsync(x => x.Login == login);
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
