using System;
using System.Collections.Generic;

namespace TaskApi.Data.Entities;

public partial class Task
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public DateTime Ts { get; set; }

    public virtual User User { get; set; } = null!;
}
