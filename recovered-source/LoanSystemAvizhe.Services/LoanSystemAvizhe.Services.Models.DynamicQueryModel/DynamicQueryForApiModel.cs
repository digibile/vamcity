namespace LoanSystemAvizhe.Services.Models.DynamicQueryModel;

public class DynamicQueryForApiModel
{
	public int Page { get; set; }

	public int ItemsPerPage { get; set; } = 50;

	public string NationalCode { get; set; }

	public string Email { get; set; }

	public string Mobile { get; set; }
}
