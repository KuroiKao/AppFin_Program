using System;
using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class Report
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public DateTimeOffset ReportDate { get; set; }

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public virtual User User { get; set; } = null!;
}
