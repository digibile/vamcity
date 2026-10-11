using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IConsultantService
{
	Task<List<Consultant>> GetAll(bool? isSeen);

	Task<int> GetCount(bool? isSeen);

	Task<Consultant> GetById(Guid entityId);

	Task Insert(Consultant entity);

	Task Update(Consultant entity);

	Task Delete(Consultant entity);
}
