using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace AppFin_Program.Factory
{
    public class DbContextFactory : IDbContextFactory<FinAppDataBaseContext>
    {
        private readonly string _connectionString;
        public DbContextFactory(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            }
            Debug.WriteLine($"Using connection string: {connectionString}");

            _connectionString = connectionString;
        }

        public FinAppDataBaseContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<FinAppDataBaseContext>();
            optionsBuilder.UseSqlServer(_connectionString);

            return new FinAppDataBaseContext(optionsBuilder.Options);
        }
    }
}
