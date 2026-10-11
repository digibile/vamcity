using Microsoft.AspNetCore.Http;

namespace LoanSystem.Site.Areas.Admin.Models;

internal class AssuranceFile
{
	public IFormFile File { get; set; }

	public string FileName { get; set; }

	public bool ForAdd { get; set; }

	public bool ForDelete { get; set; }

	public bool IsAssurance { get; set; }

	public bool IsAssuranceCheque { get; set; }
}
