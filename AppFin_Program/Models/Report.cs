using System;
using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class Report
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string FilePath { get; set; }

    public virtual User User { get; set; } = null!;
}
