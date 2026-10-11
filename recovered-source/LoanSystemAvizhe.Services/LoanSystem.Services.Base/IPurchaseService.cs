using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IPurchaseService
{
	IQueryable<Purchase> GetAllAsQueryable();

	Task<List<Purchase>> GetAll(params string[] includs);

	Task<Purchase> GetById(Guid entityId, params string[] includs);

	Task<Purchase> GetByZarinPalAuthority(string authority, params string[] includs);

	Task ApplyPurchase(Purchase purchase);

	Task Insert(Purchase entity);

	Task Update(Purchase entity);

	Task Delete(Purchase entity);
}
