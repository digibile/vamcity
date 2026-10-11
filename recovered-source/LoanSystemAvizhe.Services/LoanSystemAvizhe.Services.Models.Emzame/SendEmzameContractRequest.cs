using System.Collections.Generic;

namespace LoanSystemAvizhe.Services.Models.Emzame;

public class SendEmzameContractRequest
{
	public string FileName { get; set; }

	public string FileDescription { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }

	public string Provider { get; set; } = "gica";

	public string SignType { get; set; } = "e-sign";

	public string ContractType { get; set; } = "pdf";

	public bool IsElectronicSignatureRequired { get; set; }

	public bool IsSignatureDescriptionRequired { get; set; } = true;

	public bool ShouldEveryPageHaveSignatureImage { get; set; }

	public long ExpireTime { get; set; } = 2600000000L;

	public bool IsBlankPageRequire { get; set; }

	public List<SendEmzameContractCollaborator> CollaboratorList { get; set; }
}
