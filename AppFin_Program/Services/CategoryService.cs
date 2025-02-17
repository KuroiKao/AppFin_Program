using AppFin_Program.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppFin_Program.Services
{
    public partial class CategoryService(IDbContextFactory<FinAppDataBaseContext> dbContextFactory) : DbContextService(dbContextFactory)
    {
        public List<Category> GetCategories()
        {
            return DbContext.Categories.ToList();
        }

        public List<TransactionType> GetTransactionTypes()
        {
            return DbContext.TransactionTypes.ToList();
        }
        public TransactionType GetIncomeTransactionType()
        {            
            return DbContext.TransactionTypes.FirstOrDefault(t => t.Name == "Доход")
                   ?? throw new InvalidOperationException("Transaction type 'Доход' not found.");
        }

        public TransactionType GetExpenseTransactionType()
        {
            return DbContext.TransactionTypes.FirstOrDefault(t => t.Name == "Расход")
                   ?? throw new InvalidOperationException("Transaction type 'Расход' not found.");
        }
    }
}
