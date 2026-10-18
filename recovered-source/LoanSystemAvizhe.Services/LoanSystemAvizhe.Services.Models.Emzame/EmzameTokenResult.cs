namespace LoanSystemAvizhe.Services.Models.Emzame;

public class EmzameTokenResult : SendEmzameErrorResult
{
    public string accessToken { get; set; }

    public string refreshToken { get; set; }

    public EmzameTokenUserInfo userInfo { get; set; }
}
