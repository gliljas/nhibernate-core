#if NET6_0_OR_GREATER
using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Base class for DateOnly types.
	/// </summary>
	[Serializable]
	public abstract class AbstractDateOnlyType<TParameter> : PrimitiveType
	{
		protected AbstractDateOnlyType(SqlType sqlType) : base(sqlType)
		{
		}

		public override string Name => GetType().Name[..^4];

		public override System.Type ReturnedClass => typeof(DateOnly);

		public override System.Type PrimitiveClass => typeof(DateOnly);

		public override object DefaultValue => DateOnly.MinValue;

		/// <inheritdoc />
		public override object Get(DbDataReader rs, int index, ISessionImplementor session)
		{
			try
			{
				return GetDateOnlyFromReader(rs, index, session);
			}
			catch (Exception ex) when (ex is not FormatException)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
			}
		}

		/// <inheritdoc />
		public override object Get(DbDataReader rs, string name, ISessionImplementor session)
		{
			return Get(rs, rs.GetOrdinal(name), session);
		}

		/// <inheritdoc />
		public override void Set(DbCommand st, object value, int index, ISessionImplementor session)
		{
			st.Parameters[index].Value = GetParameterValueToSet((DateOnly) value,session);
		}

		/// <summary>
		/// Get the DateOnly value from the <see cref="DbDataReader"/> at index <paramref name="index"/>
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="index"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		protected abstract DateOnly GetDateOnlyFromReader(DbDataReader rs, int index, ISessionImplementor session);

		/// <summary>
		/// Convert <paramref name="dateOnly"/> into the <typeparamref name="TParameter"/> which will be set on the parameter
		/// </summary>
		/// <param name="dateOnly"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		protected abstract TParameter GetParameterValueToSet(DateOnly dateOnly, ISessionImplementor session);

		public override bool IsEqual(object x, object y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}

			var date1 = (DateOnly) x;
			var date2 = (DateOnly) y;
			return date1.Equals(date2);
		}

		public override int GetHashCode(object x)
		{
			var date = (DateOnly) x;
			return date.GetHashCode();
		}

		/// <inheritdoc />
		public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
		{
			return (value == null) ? null :
				// 6.0 TODO: inline this call.
#pragma warning disable 618
				ToString(value);
#pragma warning restore 618
		}

		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version. Override ToLoggableString instead.")]
		public override string ToString(object val)
		{
			return ((DateOnly) val).ToString();
		}

		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version.")]
		public override object FromStringValue(string xml)
		{
			return DateOnly.Parse(xml);
		}
	}
}
#endif
