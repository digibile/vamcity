using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface ITagService
{
    Task<List<Tag>> GetAll(Guid? customerId, TagCategory? category);

    Task<Tag> GetById(Guid entityId, params string[] includs);

    Task Insert(Tag entity);

    Task Update(Tag entity);

    Task Delete(Tag entity);

    Task<List<Tag>> GetByCondition(Expression<Func<Tag, bool>> condition);
}
