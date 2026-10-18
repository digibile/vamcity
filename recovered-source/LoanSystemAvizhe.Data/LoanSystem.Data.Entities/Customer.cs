using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanSystem.Data.Entities;

public class Customer : BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Code { get; set; }

    public string Name { get; set; }

    public string Mobile { get; set; }

    public string Email { get; set; }

    public bool FirstLoginDone { get; set; }

    public string InitPassword { get; set; }

    public DateTime RegDate { get; set; }

    public DateTime ExpireDate { get; set; }

    public bool Purchased { get; set; }

    public bool Permanent { get; set; }

    public string AffCode { get; set; }

    public string ParkDomain { get; set; }

    public virtual ICollection<Purchase> Purchases { get; set; } = new HashSet<Purchase>();
}
