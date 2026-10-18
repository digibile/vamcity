using System;

namespace LoanSystem.Services.Models.Landa;

public class LandaTransferModel
{
    public string Creator { get; set; }

    public string CreatedDateTime { get; set; }

    public Guid LandaId { get; set; }

    public string LandaCode { get; set; }

    public string LandaName { get; set; }
}
