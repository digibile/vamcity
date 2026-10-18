using System;

namespace LoanSystem.Data.Entities;

public class Consultant : BaseEntity
{
    public string Name { get; set; }

    public string Phone { get; set; }

    public string Description { get; set; }

    public bool IsSeen { get; set; }

    public string CallResult { get; set; }

    public DateTime SaveDate { get; set; }
}
