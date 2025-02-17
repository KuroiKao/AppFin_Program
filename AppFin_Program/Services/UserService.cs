using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public partial class UserService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory) : DbContextService(dbContextFactory)
    {
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            var userByLogin = await DbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
            return userByLogin;
        }

        public async Task RegisterUserAsync(string login, string password, string email)
        {
            var newUser = new User
            {
                Login = login,
                Password = password,
                Email = email
            };

            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync();
        }
    }
}
