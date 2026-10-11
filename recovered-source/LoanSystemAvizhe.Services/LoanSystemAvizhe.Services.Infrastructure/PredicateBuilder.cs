using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LoanSystemAvizhe.Services.Infrastructure;

public static class PredicateBuilder
{
	private class ParameterRebinder : ExpressionVisitor
	{
		private readonly Dictionary<ParameterExpression, ParameterExpression> map;

		private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
		{
			this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
		}

		public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
		{
			return new ParameterRebinder(map).Visit(exp);
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (map.TryGetValue(p, out var value))
			{
				p = value;
			}
			return base.VisitParameter(p);
		}
	}

	public static Expression<Func<T, bool>> True<T>()
	{
		return (T param) => true;
	}

	public static Expression<Func<T, bool>> False<T>()
	{
		return (T param) => false;
	}

	public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
	{
		return predicate;
	}

	public static Expression<Func<T, bool>> AND<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
	{
		return first.Compose(second, Expression.AndAlso);
	}

	public static Expression<Func<T, bool>> OR<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
	{
		return first.Compose(second, Expression.OrElse);
	}

	public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
	{
		return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
	}

	private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
	{
		Expression arg = ParameterRebinder.ReplaceParameters(first.Parameters.Select((ParameterExpression f, int i) => new
		{
			f = f,
			s = second.Parameters[i]
		}).ToDictionary(p => p.s, p => p.f), second.Body);
		return Expression.Lambda<T>(merge(first.Body, arg), first.Parameters);
	}
}
