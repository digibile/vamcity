using System.Collections.Generic;
using LoanSystem.Site.Helpers.Security;

namespace LoanSystemAvizhe.Site.Models;

public class OTPLoginResult
{
    public JwtToken JwtToken { get; set; }

    public List<string> Roles { get; set; }
}
