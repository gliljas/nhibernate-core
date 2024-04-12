#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps a <see cref="System.DateOnly" /> property to a <see cref="DbType.Date"/> column
	/// </summary>
	[Serializable]
	public class DateOnlyAsIntType : AbstractDateOnlyType<int>
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public DateOnlyAsIntType() : base(SqlTypeFactory.Int32)
		{
		}

		protected override DateOnly GetDateOnlyFromReader(DbDataReader rs, int index, ISessionImplementor session)
		{
			var intVal = rs.GetInt32(index);
			if (intVal > 99991231 || intVal < 0)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]));
			}
			return new DateOnly(intVal / 10000, intVal % 10000 / 100, intVal % 100);
		}

		protected override int GetParameterValueToSet(DateOnly dateOnly, ISessionImplementor session) => GetDateOnlyAsInt(dateOnly);

		private int GetDateOnlyAsInt(DateOnly dateOnly) => dateOnly.Year * 10000 + dateOnly.Month * 100 + dateOnly.Day;

		/// <inheritdoc />
		public override string ObjectToSQLString(object value, Dialect.Dialect dialect) => GetDateOnlyAsInt((DateOnly)value).ToString();
	}
}
#endif
