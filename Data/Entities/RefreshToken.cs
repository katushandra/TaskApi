using System;
using System.Collections.Generic;

namespace TaskApi.Data.Entities;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string TokenSalt { get; set; } = null!;

    public DateTime Ts { get; set; }

    public DateTime ExpiryDate { get; set; }

    public virtual User User { get; set; } = null!;
}
