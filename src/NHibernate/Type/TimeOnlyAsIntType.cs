#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{

	/// <summary>
	/// Maps a <see cref="System.TimeOnly" /> property to a <see cref="DbType.Int32" /> column.
	/// The value persisted is the hhmmss representation of the time, expressed as an integer.
	/// </summary>
	[Serializable]
	public class TimeOnlyAsIntType : AbstractTimeOnlyType<int>
	{
		public TimeOnlyAsIntType()
			: base(0, SqlTypeFactory.Int32)
		{
		}

		protected override TimeOnly GetTimeOnlyFromReader(DbDataReader rs, int index, ISessionImplementor session)
		{
			var intVal = rs.GetInt32(index);
			if (intVal > 235959 || intVal < 0)
			{
				throw new FormatException(string.Format("Input value '{0}' was not in the correct format.", rs[index]));
			}
			return new TimeOnly(intVal / 10000, intVal % 10000 / 100, intVal % 100);
		}

		protected override int GetParameterValueToSet(TimeOnly timeOnly, ISessionImplementor session) =>
			(timeOnly.Hour * 10000) + (timeOnly.Minute * 100) + timeOnly.Second;

		public override string ObjectToSQLString(object value, Dialect.Dialect dialect) =>
			((TimeOnly) value).ToString("hhmmss");
	}
}
#endif
