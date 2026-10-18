namespace LoanSystem.Site.Helpers.Security;

public class JwtToken
{
    public string AccessToken { get; set; }

    public int ExpiredIn { get; set; }

    public string UserData { get; set; }

    public string Permissions { get; set; }
}
