using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IContactService
{
    Task<Contact> GetById(Guid entityId, params string[] includs);

    Task<List<Contact>> GetByCondition(Expression<Func<Contact, bool>> condition, params string[] includs);

    Task Insert(Contact entity);

    Task Update(Contact entity);

    Task Delete(Contact entity);
}
