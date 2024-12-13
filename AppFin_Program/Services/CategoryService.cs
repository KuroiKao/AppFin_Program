using AppFin_Program.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppFin_Program.Services
{
    public class CategoryService
    {
        private readonly FinAppDataBaseContext _dbContext;

        public CategoryService(FinAppDataBaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Category> GetCategories()
        {
            return _dbContext.Categories.ToList();
        }

        public List<TransactionType> GetTransactionTypes()
        {
            return _dbContext.TransactionTypes.ToList();
        }
        public TransactionType GetIncomeTransactionType()
        {
            return _dbContext.TransactionTypes.FirstOrDefault(t => t.Name == "Доход")
                   ?? throw new InvalidOperationException("Transaction type 'Доход' not found.");
        }

        public TransactionType GetExpenseTransactionType()
        {
            return _dbContext.TransactionTypes.FirstOrDefault(t => t.Name == "Расход")
                   ?? throw new InvalidOperationException("Transaction type 'Расход' not found.");
        }
    }
}
