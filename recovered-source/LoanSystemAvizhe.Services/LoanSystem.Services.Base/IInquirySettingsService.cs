using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IInquirySettingsService
{
    Task<List<InquirySettings>> GetAll();

    Task<InquirySettings> GetById(Guid entityId, params string[] includs);

    Task Insert(InquirySettings entity);

    Task Update(InquirySettings entity);

    Task Delete(InquirySettings entity);

    Task<int> GetCountByCondition(Expression<Func<InquirySettings, bool>> condition);

    Task<List<InquirySettings>> GetByCondition(Expression<Func<InquirySettings, bool>> condition);
}
