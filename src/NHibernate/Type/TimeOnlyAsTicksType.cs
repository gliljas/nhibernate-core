#if NET6_0_OR_GREATER
using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{

	/// <summary>
	/// Maps a <see cref="System.TimeOnly" /> property to a <see cref="DbType.Int64" /> column.
	/// The value persisted is the Ticks property of the TimeOnly value.
	/// </summary>
	[Serializable]
	public class TimeOnlyAsTicksType : AbstractTimeOnlyType<long>
	{
		/// <summary>
		/// Default constructor. Sets the fractional seconds precision (scale) to 0
		/// </summary>
		public TimeOnlyAsTicksType() : this(0)
		{
		}

		/// <summary>
		/// Constructor for specifying a fractional seconds precision (scale).
		/// </summary>
		/// <param name="fractionalSecondsPrecision">The fractional seconds precision. Any value beyond 7 is pointless, since it's the maximum precision allowed by .NET</param>
		public TimeOnlyAsTicksType(byte fractionalSecondsPrecision) : base(fractionalSecondsPrecision, SqlTypeFactory.Int64)
		{
		}

		protected override TimeOnly GetTimeOnlyFromReader(DbDataReader rs, int index, ISessionImplementor session) => new(rs.GetInt64(index));

		protected override long GetParameterValueToSet(TimeOnly timeOnly, ISessionImplementor session) => timeOnly.Ticks;

		public override string ObjectToSQLString(object value, Dialect.Dialect dialect) =>
			AdjustTimeOnly((TimeOnly) value).Ticks.ToString();
	}
}
#endif
