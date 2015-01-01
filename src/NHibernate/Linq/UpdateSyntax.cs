using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;

namespace NHibernate.Linq
{
	public class UpdateSyntax<T>
	{
		private readonly IQueryable<T> _query;

		internal UpdateSyntax(IQueryable<T> query)
		{
			_query = query;
		}


		/// <summary>
		/// Specify the assignments and execute the update.
		/// </summary>
		/// <param name="assignments">The assignments.</param>
		/// <param name="versioned">if set to <c>true</c> [versioned].</param>
		/// <returns></returns>
		public int Assign(Action<Assignments<T, T>> assignments, bool versioned = false)
		{
			var u = new Assignments<T, T>();
			assignments.Invoke(u);

			return ExecuteUpdate(versioned, u);
		}

		/// <summary>
		/// Specify the assignments and execute the update.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="expression">The assignments expressed as a member initialization, e.g. x => new Dog{Name = x.Name,Age = x.Age + 5}.</param>
		/// <param name="versioned">if set to <c>true</c> [versioned].</param>
		/// <returns></returns>
		public int As(Expression<Func<T, T>> expression, bool versioned = false)
		{

			var assignments = Assignments<T, T>.FromExpression(expression);
			return ExecuteUpdate(versioned, assignments);
		}

		private int ExecuteUpdate<T>(bool versioned, Assignments<T, T> assignments)
		{
			var nhQueryable = _query as QueryableBase<T>;
			if (nhQueryable == null)
				throw new NotSupportedException("Query needs to be of type QueryableBase<T>");

			var provider = (DefaultQueryProvider)nhQueryable.Provider;
			return provider.ExecuteUpdate<T>(nhQueryable, assignments, versioned);
		}
	}
}