using System;
using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class Goal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public decimal TargetAmount { get; set; }

    public virtual User User { get; set; } = null!;
}
