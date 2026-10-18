namespace LoanSystem.Services.Models;

public class SqlTableVColumnInfoModel
{
    public string Name { get; set; }

    public string Default { get; set; }

    public bool IsNullable { get; set; }

    public string DataType { get; set; }
}
