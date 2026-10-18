using Microsoft.EntityFrameworkCore.Storage;

namespace Spm.Services.Infrastructure;

public class DbTransaction
{
    private readonly IDbContextTransaction _transaction;

    public bool Committed { get; private set; }

    public bool RolledBack { get; private set; }

    public bool Completed
    {
        get
        {
            if (!Committed)
            {
                return RolledBack;
            }
            return true;
        }
    }

    public DbTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public void Commit()
    {
        try
        {
            _transaction.Commit();
        }
        finally
        {
            Committed = true;
        }
    }

    public void Rollback()
    {
        try
        {
            _transaction.Rollback();
        }
        finally
        {
            RolledBack = true;
        }
    }
}
