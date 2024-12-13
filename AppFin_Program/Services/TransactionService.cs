using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppFin_Program.Services
{
    public class TransactionService
    {
        private readonly FinAppDataBaseContext _dbContext; 
        private readonly UserSessionService _userSessionService;


        public TransactionService(FinAppDataBaseContext dbContext, UserSessionService userSessionService)
        {
            _dbContext = dbContext;
            _userSessionService = userSessionService;
        }

        public void AddTransaction(Category selectedCategory, TransactionType selectedType, decimal amount, DateTimeOffset? selectedDate)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            try
            {
                var transactionCategory = new TransactionCategory
                {
                    CategoryId = selectedCategory.Id,
                    TransactionTypeId = selectedType.Id
                };

                _dbContext.TransactionCategories.Add(transactionCategory);
                _dbContext.SaveChanges();

                var transactionRecord = new Transaction
                {
                    UserId = _userSessionService.GetCurrentUserId(),
                    Amount = amount,
                    TransactionDate = selectedDate ?? DateTimeOffset.Now,
                    TransactionCategoriesId = transactionCategory.Id
                };

                _dbContext.Transactions.Add(transactionRecord);
                _dbContext.SaveChanges();

                transaction.Commit();
                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка загрузки транзакции", ex);
            }
        }
        public List<Transaction> LoadTransactions()
        {
            return _dbContext.Transactions
                .Include(t => t.TransactionCategories)
                .ThenInclude(tc => tc.Category)
                .Where(t => t.UserId == _userSessionService.GetCurrentUserId())
                .ToList();
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
            var transactionsQuery = _dbContext.Transactions
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
