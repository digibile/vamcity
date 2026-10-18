using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Spm.Services.Infrastructure;

public abstract class BaseService
{
    private readonly ConcurrentDictionary<string, object> _entitySets = new ConcurrentDictionary<string, object>();

    protected AppDbContext _context { get; private set; }

    protected ICurrentUserService _currentUserService { get; private set; }

    public BaseService(AppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    protected virtual void Insert<T>(T entity) where T : BaseEntity
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        Entities<T>().Add(entity);
    }

    protected virtual void InsertBulk<T>(List<T> entities) where T : BaseEntity
    {
        if (entities == null)
        {
            throw new ArgumentNullException("T");
        }
        Entities<T>().AddRange(entities);
    }

    protected virtual void Insert<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentException("entities");
        }
        Entities<T>().AddRange(entities);
    }

    protected virtual void Update<T>(T entity, bool update = true, params Expression<Func<T, object>>[] updatedProperties) where T : BaseEntity
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        EntityEntry<T> entry = _context.Entry(entity);
        if (entry == null)
        {
            EntityEntry<T> entityEntry = _context.ChangeTracker.Entries<T>().FirstOrDefault((EntityEntry<T> x) => x.Entity.Id == entity.Id);
            if (entityEntry != null)
            {
                entityEntry.State = EntityState.Detached;
            }
            Entities<T>().Attach(entity);
            entry = _context.Entry(entity);
        }
        entry.State = EntityState.Modified;
        if (updatedProperties == null || updatedProperties.Length == 0)
        {
            return;
        }
        List<string> list = updatedProperties.Select((Expression<Func<T, object>> prop) =>
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
        List<string> first = (from m in entry.Metadata.GetProperties().ToArray()
                              select m.Name).ToList();
        if (update)
        {
            first.Except(list).ToList().ForEach((string x) =>
            {
                entry.Property(x).IsModified = false;
            });
        }
        else
        {
            list.ForEach((string x) =>
            {
                entry.Property(x).IsModified = false;
            });
        }
    }

    protected virtual void Update<T>(IEnumerable<T> entities, bool update = true, params Expression<Func<T, object>>[] updatedProperties) where T : BaseEntity
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentException("entities");
        }
        Entities<T>().AttachRange(entities);
        foreach (T entity2 in entities)
        {
            _context.Entry(entity2).State = EntityState.Modified;
        }
        if (updatedProperties == null || updatedProperties.Length == 0)
        {
            return;
        }
        List<string> list = updatedProperties.Select((Expression<Func<T, object>> prop) =>
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
        T entity = entities.First();
        List<string> list2 = (from m in _context.Entry(entity).Metadata.GetProperties().ToArray()
                              select m.Name).ToList().Except(list).ToList();
        foreach (T entity3 in entities)
        {
            EntityEntry<T> entry = _context.Entry(entity3);
            if (update)
            {
                list2.ForEach((string x) =>
                {
                    entry.Property(x).IsModified = false;
                });
            }
            else
            {
                list.ForEach((string x) =>
                {
                    entry.Property(x).IsModified = false;
                });
            }
        }
    }

    protected virtual void Delete<T>(T entity) where T : BaseEntity
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        Entities<T>().Remove(entity);
    }

    protected virtual void Delete<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentException("entities");
        }
        Entities<T>().RemoveRange(entities);
    }

    protected Task<int> Save(object cacheGroupKey = null)
    {
        return _context.SaveChangesAsync();
    }

    protected virtual IQueryable<T> Table<T>() where T : BaseEntity
    {
        return Entities<T>().AsNoTracking();
    }

    protected virtual IQueryable<T> TableTracking<T>() where T : BaseEntity
    {
        return Entities<T>();
    }

    protected virtual DbSet<T> Entities<T>() where T : BaseEntity
    {
        return _entitySets.GetOrAdd(typeof(T).FullName, (string key) => _context.Set<T>()) as DbSet<T>;
    }

    protected virtual Task<T> GetById<T>(Guid entityId) where T : BaseEntity
    {
        return Table<T>().SingleOrDefaultAsync((T x) => x.Id == entityId);
    }

    protected virtual Task<T> GetByIdNoTrack<T>(Guid entityId, params string[] includs) where T : BaseEntity
    {
        IQueryable<T> source = _context.Set<T>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.AsNoTracking().AsSplitQuery().SingleOrDefaultAsync((T x) => x.Id == entityId);
    }

    protected virtual Task<T> GetById<T>(Guid entityId, params string[] includs) where T : BaseEntity
    {
        IQueryable<T> source = _context.Set<T>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.AsSplitQuery().SingleOrDefaultAsync((T x) => x.Id == entityId);
    }

    protected async Task RunInTransaction(Func<DbTransaction, Task> action)
    {
        using IDbContextTransaction scope = await _context.Database.BeginTransactionAsync();
        DbTransaction transaction = new DbTransaction(scope);
        await action(transaction);
        if (!transaction.Completed)
        {
            transaction.Commit();
        }
    }

    protected async Task<T> RunInTransaction<T>(Func<DbTransaction, Task<T>> action)
    {
        using IDbContextTransaction scope = await _context.Database.BeginTransactionAsync();
        DbTransaction transaction = new DbTransaction(scope);
        T result = await action(transaction);
        if (!transaction.Completed)
        {
            transaction.Commit();
        }
        return result;
    }

    protected void RunInTransaction(Action<DbTransaction> action)
    {
        using IDbContextTransaction transaction = _context.Database.BeginTransaction();
        DbTransaction dbTransaction = new DbTransaction(transaction);
        action(dbTransaction);
        if (!dbTransaction.Completed)
        {
            dbTransaction.Commit();
        }
    }

    protected T RunInTransaction<T>(Func<DbTransaction, T> action)
    {
        using IDbContextTransaction transaction = _context.Database.BeginTransaction();
        DbTransaction dbTransaction = new DbTransaction(transaction);
        T result = action(dbTransaction);
        if (!dbTransaction.Completed)
        {
            dbTransaction.Commit();
        }
        return result;
    }

    private void WriteProgress(decimal percentage)
    {
    }
}
