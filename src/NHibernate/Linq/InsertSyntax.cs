using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Utilities;

namespace NHibernate.Linq
{
	public class InsertSyntax<TInput>
	{
		private readonly IQueryable<TInput> _query;

		internal InsertSyntax(IQueryable<TInput> query)
		{
			_query = query;
		}

		/// <summary>
		/// Executes the insert, using the specified assignments.
		/// </summary>
		/// <typeparam name="TOutput">The type of the output.</typeparam>
		/// <param name="assignmentActions">The assignments.</param>
		/// <returns></returns>
		public int Into<TOutput>(Action<Assignments<TInput, TOutput>> assignmentActions)
		{
			ArgumentUtility.CheckNotNull("assignments", assignmentActions);
			var assignments = new Assignments<TInput, TOutput>();
			assignmentActions.Invoke(assignments);
			return InsertInto(_query, assignments);
		}

		/// <summary>
		/// Executes the insert, inserting new entities as specified by the expression
		/// </summary>
		/// <typeparam name="TOutput">The type of the output.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns></returns>
		public int As<TOutput>(Expression<Func<TInput, TOutput>> expression)
		{
			var assignments = Assignments<TInput, TOutput>.FromExpression(expression);
			return InsertInto(_query, assignments);
		}

		internal int InsertInto<TOutput>(IQueryable<TInput> query, Assignments<TInput, TOutput> assignments)
		{
			var nhQueryable = query as QueryableBase<TInput>;
			if (nhQueryable == null)
				throw new NotSupportedException("Query needs to be of type QueryableBase<T>");

			var provider = (DefaultQueryProvider)query.Provider;
			return provider.ExecuteInsert(nhQueryable, assignments);
		}

	}
}