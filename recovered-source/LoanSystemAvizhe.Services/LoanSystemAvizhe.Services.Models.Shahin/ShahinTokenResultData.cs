using System.Collections.Generic;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinTokenResultData
{
    public string Access_Token { get; set; }

    public string Token_Type { get; set; }

    public long Expires_in { get; set; }

    public List<string> Scope { get; set; }

    public string Bank { get; set; }

    public List<string> Accounts { get; set; }

    public long Amount { get; set; }

    public string Jti { get; set; }

    public string User_Name { get; set; }

    public long Iat { get; set; }

    public long Nbf { get; set; }
}
