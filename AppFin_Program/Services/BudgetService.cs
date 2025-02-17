using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public partial class BudgetService : DbContextService
    {
        private readonly UserSessionService _userSessionService;

        public BudgetService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory, UserSessionService userSessionService) : base(dbContextFactory) 
        {
            _userSessionService = userSessionService;
        }
        public async Task<Budget> GetCurrentBudgetAsync()
        {
            var userId = _userSessionService.GetCurrentUserId();
            var budget = await DbContext.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (budget == null)
            {
                budget = new Budget { UserId = userId, Amount = 0 };
                await DbContext.Budgets.AddAsync(budget);
                await DbContext.SaveChangesAsync();
            }

            return budget;
        }
        public async Task UpdateBudgetAsync(int userId, decimal amount, bool isIncome)
        {
            var budget = await DbContext.Budgets.FirstOrDefaultAsync(b => b.UserId == userId);
            if (budget == null)
            {
                budget = new Budget
                {
                    UserId = userId,
                    Amount = 0
                };
                await DbContext.Budgets.AddAsync(budget);
            }
            budget.Amount += isIncome ? amount : -amount;

            await DbContext.SaveChangesAsync();
        }
        public async Task RecalculateBudgetAsync()
        {
            try
            {
                var userId = _userSessionService.GetCurrentUserId();

                var transactions = await DbContext.Transactions
                    .Include(t => t.TransactionCategories.TransactionType)
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                var totalAmount = transactions.Sum(t =>
                t.TransactionCategories.TransactionType.Name == "Доход" ? t.Amount : -t.Amount);

                var budget = await DbContext.Budgets.FirstOrDefaultAsync(b => b.UserId == userId);

                if (budget == null)
                {
                    budget = new Budget
                    {
                        UserId = userId,
                        Amount = totalAmount
                    };

                    await DbContext.Budgets.AddAsync(budget);
                }
                else
                {
                    budget.Amount = totalAmount;
                    DbContext.Budgets.Update(budget);
                }
                await DbContext.SaveChangesAsync();

                // Проверка достижения целей
                var goals = await DbContext.Goals.Where(g => g.UserId == userId).ToListAsync();
                foreach (var goal in goals)
                {
                    if (budget.Amount >= goal.TargetAmount)
                    {
                        // Логика уведомления или другие действия
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка пересчета бюджета", ex);
            }
        }
    }
}
