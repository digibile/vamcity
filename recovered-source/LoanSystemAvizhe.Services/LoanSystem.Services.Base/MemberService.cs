using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Users;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class MemberService : BaseService, IMemberService
{
	private readonly UserManager _userManager;

	public MemberService(AppDbContext context, ICurrentUserService currentUserService, UserManager userManager)
		: base(context, currentUserService)
	{
		_userManager = userManager;
	}

	public async Task<IQueryable<User>> AsQueryable()
	{
		Role memberRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync((Role x) => x.Name == "Member");
		IQueryable<long> userIds = from x in _context.UserRoles.AsNoTracking()
			where x.RoleId == memberRole.Id
			select x.UserId;
		return from x in _context.Users.AsNoTracking()
			where userIds.Contains(x.Id)
			select x;
	}

	public async Task<List<User>> GetAllMembers()
	{
		return (await _userManager.GetUsersInRoleAsync("Member")).ToList();
	}

	public async Task<List<User>> GetByCondition(Expression<Func<User, bool>> condition, params string[] includs)
	{
		Role memberRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync((Role x) => x.Name == "Member");
		IQueryable<long> userIds = from x in _context.UserRoles.AsNoTracking()
			where x.RoleId == memberRole.Id
			select x.UserId;
		IQueryable<User> source = from x in _context.Users.AsNoTracking()
			where userIds.Contains(x.Id)
			select x;
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return await source.ToListAsync();
	}

	public async Task<List<User>> SearchMembers(string search)
	{
		List<User> members = new List<User>();
		if (string.IsNullOrEmpty(search))
		{
			return members;
		}
		Role membersRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync((Role x) => x.Name == "Member");
		if (membersRole != null)
		{
			List<long> userIds = await (from x in _context.UserRoles.AsNoTracking()
				where x.RoleId == membersRole.Id
				select x.UserId).ToListAsync();
			members = await (from x in _context.Users.AsNoTracking().IgnoreQueryFilters()
				where userIds.Contains(x.Id) && (x.FirstName.Contains(search) || x.LastName.Contains(search) || string.Concat(x.FirstName + " ", x.LastName).Contains(search) || x.Mobile.Contains(search) || x.NationalCode.Contains(search))
				select x).ToListAsync();
		}
		return members;
	}

	public Task<User> GetById(long entityId, params string[] includs)
	{
		IQueryable<User> source = _context.Users;
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		return source.SingleOrDefaultAsync((User x) => x.Id == entityId);
	}

	public Task<User> GetByNationalCode(string nationalCode)
	{
		return _context.Users.FirstOrDefaultAsync((User x) => x.NationalCode == nationalCode);
	}

	public Task<User> GetByUserCode(string code)
	{
		return _context.Users.FirstOrDefaultAsync((User x) => x.UserCode == code);
	}

	public Task<User> GetByMobile(string mobile)
	{
		return _context.Users.FirstOrDefaultAsync((User x) => x.Mobile == mobile);
	}

	public Task<List<User>> GetByAgentId(long agentId)
	{
		return _context.Users.Where((User x) => x.AgentId == (long?)agentId).ToListAsync();
	}

	public Task<List<User>> GetByCustomerMarketerId(long customerMarketerId)
	{
		return _context.Users.Where((User x) => x.CustomerMarketerId == (long?)customerMarketerId).ToListAsync();
	}

	public Task Insert(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.Users.Add(entity);
		return Save();
	}

	public Task Update(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		_context.Users.Update(entity);
		return Save();
	}

	public Task Update(List<User> entities, params Expression<Func<User, object>>[] updatedProperties)
	{
		foreach (User entity2 in entities)
		{
			EntityEntry<User> entityEntry = _context.Entry(entity2);
			if (entityEntry == null)
			{
				_context.Users.Attach(entity2);
				entityEntry = _context.Entry(entity2);
			}
			entityEntry.State = EntityState.Modified;
		}
		if (updatedProperties != null && updatedProperties.Length != 0)
		{
			List<string> second = updatedProperties.Select((Expression<Func<User, object>> prop) =>
			{
				if (prop.Body is MemberExpression memberExpression)
				{
					if (memberExpression.NodeType != ExpressionType.MemberAccess || memberExpression.Member == null || memberExpression.Member.MemberType != MemberTypes.Property)
					{
						throw new ArgumentException("The expression must get a member (property).", "updatedProperties");
					}
					return memberExpression.Member.Name;
				}
				if (prop.Body is UnaryExpression { Operand: MemberExpression operand })
				{
					return operand.Member.Name;
				}
				throw new ArgumentException("The expression must get a member (property).", "updatedProperties");
			}).ToList();
			User entity = entities.First();
			List<string> list = (from m in _context.Entry(entity).Metadata.GetProperties().ToArray()
				select m.Name).ToList().Except(second).ToList();
			foreach (User entity3 in entities)
			{
				EntityEntry<User> entry = _context.Entry(entity3);
				list.ForEach((string x) =>
				{
					entry.Property(x).IsModified = false;
				});
			}
		}
		return Save();
	}

	public Task Delete(User entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		IQueryable<UserLoanPayment> entities = _context.UserLoanPayments.Where((UserLoanPayment x) => x.UserLoan.UserId == entity.Id);
		_context.UserLoanPayments.RemoveRange(entities);
		IQueryable<UserLoan> entities2 = _context.UserLoans.Where((UserLoan x) => x.UserId == entity.Id);
		_context.UserLoans.RemoveRange(entities2);
		_context.Users.Remove(entity);
		return Save();
	}
}
