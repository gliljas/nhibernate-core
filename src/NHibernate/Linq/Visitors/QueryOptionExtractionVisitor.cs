using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.Visitors
{
	internal class QueryOptionExtractionVisitor : ExpressionVisitor
	{
		private readonly QueryOptions _queryOptions;
		private bool _inFirstCallChain = true;
		private readonly Stack<Tuple<Expression, bool>> _expressionStack = new Stack<Tuple<Expression, bool>>();

		public static Expression ExtractOptions(Expression expression, QueryOptions queryOptions)
		{
			return new QueryOptionExtractionVisitor(queryOptions).Visit(expression);
		}

		public QueryOptionExtractionVisitor(QueryOptions queryOptions)
		{
			_queryOptions = queryOptions;
		}

		public override Expression Visit(Expression node)
		{
			try
			{
				var inChain = true;
				if (_expressionStack.Any())
				{
					inChain = _expressionStack.Peek().Item2;
					if (!(inChain && IsQueryableExtensionCall(_expressionStack.Peek().Item1) && IsQueryableExtensionCall(node)))
					{
						inChain = false;
					}
				}
				_expressionStack.Push(new Tuple<Expression, bool>(node, inChain));
				_inFirstCallChain = inChain;
				return base.Visit(node);
			}
			finally
			{
				_expressionStack.Pop();
			}

		}

		private bool IsQueryableExtensionCall(Expression expression)
		{
			var call = expression as MethodCallExpression;
			return call != null && call.Arguments.Any() && typeof(IQueryable).IsAssignableFrom(call.Arguments.First().Type);
		}


		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			if (expression.Method.DeclaringType == typeof(LinqExtensionMethods))
			{

				switch (expression.Method.Name)
				{
					case "Cacheable":

						if (_inFirstCallChain)
						{
							_queryOptions.Cacheable = true;
						}
						return Visit(expression.Arguments.First());

					case "CacheRegion":

						if (_inFirstCallChain)
						{
							var constant = expression.Arguments[1] as ConstantExpression;
							if (constant != null)
							{
								_queryOptions.CacheRegion = (string)constant.Value;
							}
						}
						return Visit(expression.Arguments.First());

					case "Timeout":

						if (_inFirstCallChain)
						{
							var constant = expression.Arguments[1] as ConstantExpression;
							if (constant != null)
							{
								_queryOptions.Timeout = (int)constant.Value;
							}
						}
						return Visit(expression.Arguments.First());

					case "ReadOnly":

						if (_inFirstCallChain)
						{
							var constant = expression.Arguments[1] as ConstantExpression;
							if (constant != null)
							{
								_queryOptions.ReadOnly = (bool)constant.Value;
							}
						}
						return Visit(expression.Arguments.First());

					case "LockMode":

						if (_inFirstCallChain)
						{
							var constant = expression.Arguments[1] as ConstantExpression;
							if (constant != null)
							{
								_queryOptions.LockMode = (LockMode)constant.Value;
							}
						}
						return Visit(expression.Arguments.First());

					case "Comment":

						if (_inFirstCallChain)
						{
							var constant = expression.Arguments[1] as ConstantExpression;
							if (constant != null)
							{
								_queryOptions.Comment = (string)constant.Value;
							}
						}
						return Visit(expression.Arguments.First());
				}
			}
			return base.VisitMethodCall(expression);
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			//Exit early
			if (typeof(IQueryable).IsAssignableFrom(node.Type))
			{
				var expression = Visit(node.Expression);

				var constantExpression = expression as ConstantExpression;
				if (constantExpression != null)
				{
					object container = constantExpression.Value;
					var member = node.Member;
					object value = null;
					var info = member as FieldInfo;
					if (info != null)
					{
						value = info.GetValue(container);
					}
					else
					{
						var propertyInfo = member as PropertyInfo;
						if (propertyInfo != null)
						{
							value = propertyInfo.GetValue(container, null);
						}
					}
					var subQuery = value as IQueryable;
					if (subQuery != null)
					{
						value = subQuery.Provider.CreateQuery(Visit(subQuery.Expression));
					}
					return Expression.Constant(value);
				}
			}
			return base.VisitMember(node);
		}

	}

}
