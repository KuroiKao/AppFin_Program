using System;

namespace AppFin_Program.Models;

public partial class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TransactionCategoriesId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }

    public virtual TransactionCategory TransactionCategories { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
