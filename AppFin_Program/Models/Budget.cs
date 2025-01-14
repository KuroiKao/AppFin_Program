﻿using System;
using System.Collections.Generic;

namespace AppFin_Program.Models;

public partial class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
