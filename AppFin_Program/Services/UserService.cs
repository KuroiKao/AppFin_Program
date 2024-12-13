using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public class UserService
    {
        private readonly FinAppDataBaseContext _dbContext;

        public UserService(FinAppDataBaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserByLoginAsync(string login)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == login);
        }

        public async Task RegisterUserAsync(string login, string password, string email)
        {
            var newUser = new User
            {
                Login = login,
                Password = password,
                Email = email
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
        }
    }
}
