using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystemAvizhe.Services.ThirdParty;

public interface IOnlineMoneyTransferService
{
    Task<List<OnlineMoneyTransfer>> GetByCondition(Expression<Func<OnlineMoneyTransfer, bool>> condition, params string[] includs);

    Task<OnlineMoneyTransfer> GetById(Guid entityId, params string[] includs);

    Task Insert(OnlineMoneyTransfer entity);

    Task Update(OnlineMoneyTransfer entity);

    Task Delete(OnlineMoneyTransfer entity);
}
