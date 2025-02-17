using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class TransactionCategory
{
    public int Id { get; set; }

    public int TransactionTypeId { get; set; }

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
