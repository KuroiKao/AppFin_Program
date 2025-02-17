using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class TransactionType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TransactionCategory> TransactionCategories { get; set; } = new List<TransactionCategory>();
}
