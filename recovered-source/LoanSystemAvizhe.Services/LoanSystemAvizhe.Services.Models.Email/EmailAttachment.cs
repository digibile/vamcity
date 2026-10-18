using System.IO;

namespace LoanSystemAvizhe.Services.Models.Email;

public class EmailAttachment
{
    public string Name { get; set; }

    public Stream Stream { get; set; }
}
