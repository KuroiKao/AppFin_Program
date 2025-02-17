using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public partial class TransactionService : DbContextService
    {
        private readonly UserSessionService _userSessionService;
        private readonly BudgetService _budgetService;
        public TransactionService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory, UserSessionService userSessionService, BudgetService budgetService) : base(dbContextFactory)
        {
            _userSessionService = userSessionService;
            _budgetService = budgetService;
        }

        public async Task AddTransactionAsync(Category selectedCategory, TransactionType selectedType, decimal amount, DateTimeOffset? selectedDate)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync();

            try
            {
                var transactionCategory = new TransactionCategory
                {
                    CategoryId = selectedCategory.Id,
                    TransactionTypeId = selectedType.Id
                };

                await DbContext.TransactionCategories.AddAsync(transactionCategory);
                await DbContext.SaveChangesAsync();

                var transactionRecord = new Transaction
                {
                    UserId = _userSessionService.GetCurrentUserId(),
                    Amount = amount,
                    TransactionDate = selectedDate ?? DateTimeOffset.Now,
                    TransactionCategoriesId = transactionCategory.Id
                };

                await DbContext.Transactions.AddAsync(transactionRecord);
                await DbContext.SaveChangesAsync();

                bool isIncome = selectedType.Name == "Доход";
                await _budgetService.UpdateBudgetAsync(transactionRecord.UserId, amount, isIncome);

                await transaction.CommitAsync();                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка загрузки транзакции", ex);
            }
        }
        public List<Transaction> LoadTransactions()
        {
            return DbContext.Transactions
                .Include(t => t.TransactionCategories)
                .ThenInclude(tc => tc.Category)
                .Where(t => t.UserId == _userSessionService.GetCurrentUserId())
                .OrderByDescending(t => t.TransactionDate)
                .ToList(); ;
        }
        public List<TransactionDisplayModel> GetTransactionDisplayModels(List<Transaction> transactions)
        {
            return transactions.Select(transaction => new TransactionDisplayModel
            {
                Id = transaction.Id,
                CategoryName = transaction.TransactionCategories.Category.Name,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate.Date
            }).ToList();
        }
        public async Task<List<Transaction>> GetTransactionsAsync(DateTimeOffset startDate, DateTimeOffset endDate, bool includeExpenses, bool includeIncomes)
        {
            var transactionsQuery = DbContext.Transactions
                .Include(t => t.TransactionCategories.Category)
                .Include(t => t.TransactionCategories.TransactionType)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            if (includeExpenses && !includeIncomes)
                transactionsQuery = transactionsQuery.Where(t => t.TransactionCategories.TransactionType.Name == "Расход");

            if (!includeExpenses && includeIncomes)
                transactionsQuery = transactionsQuery.Where(t => t.TransactionCategories.TransactionType.Name == "Доход");

            return await transactionsQuery.ToListAsync();
        }
    }
}
