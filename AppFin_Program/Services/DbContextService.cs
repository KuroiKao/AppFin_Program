using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppFin_Program.Services
{
    public abstract class DbContextService : IDisposable
    {
        private readonly IDbContextFactory<FinAppDataBaseContext> _dbContextFactory;
        private FinAppDataBaseContext? _dbContext;

        protected FinAppDataBaseContext DbContext => _dbContext ??= _dbContextFactory.CreateDbContext();

        protected DbContextService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
