using System;

namespace AppFin_Program.Models
{
    public class TransactionDisplayModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset TransactionDate { get; set; }
    }
}
