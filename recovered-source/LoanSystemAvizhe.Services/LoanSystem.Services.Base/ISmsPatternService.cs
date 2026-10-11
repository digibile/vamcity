using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Base;

public interface ISmsPatternService
{
	Task<List<SmsPattern>> GetAll(Guid? customerId, SmsPatternStatus? Status, params string[] includs);

	Task<int> GetCount(Guid? customerId, SmsPatternStatus? Status);

	Task<SmsPattern> GetById(Guid entityId, params string[] includs);

	Task Insert(SmsPattern entity, Guid? customerId = null);

	Task Update(SmsPattern entity);

	Task Delete(SmsPattern entity);
}
