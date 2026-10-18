using System;

namespace LoanSystem.Data.Entities;

public class AssuranceChequeItem : BaseEntity
{
    public DateTime DueDate { get; set; }

    public string FileName { get; set; }

    public string ChequeNumber { get; set; }

    public string SayadiId { get; set; }

    public decimal Price { get; set; }

    public bool IsCollected { get; set; }

    public bool IsReturned { get; set; }

    public Guid AssuranceId { get; set; }

    public virtual Assurance Assurance { get; set; }
}
