#if NET6_0_OR_GREATER
using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Base class for TimeOnly types.
	/// </summary>
	[Serializable]
	public abstract class AbstractTimeOnlyType<TParameter> : PrimitiveType
	{
		private int _ticksForPrecision;
		private readonly byte? _fractionalSecondsPrecision;

		protected AbstractTimeOnlyType(byte? fractionalSecondsPrecision, SqlType sqlType) : base(sqlType)
		{
			_ticksForPrecision = (fractionalSecondsPrecision ?? 0) switch
			{
				0 => 10000000,
				1 => 1000000,
				2 => 100000,
				3 => 10000,
				4 => 1000,
				5 => 100,
				6 => 10,
				_ => 1,
			};
			_fractionalSecondsPrecision = fractionalSecondsPrecision;
		}


		public override string Name => GetType().Name[..^4];

		public override System.Type ReturnedClass => typeof(TimeOnly);

		public override System.Type PrimitiveClass => typeof(TimeOnly);

		public override object DefaultValue => TimeOnly.MinValue;

		/// <summary>
		/// Adjust <paramref name="timeOnly"/> according to the fractional seconds precision defined
		/// If overridden, the base implementation should usually be called
		/// </summary>
		/// <param name="timeOnly"></param>
		/// <returns></returns>
		protected virtual TimeOnly AdjustTimeOnly(TimeOnly timeOnly)
		{
			long remainder;
			if (_fractionalSecondsPrecision < 7 && (remainder = timeOnly.Ticks % _ticksForPrecision) > 0)
			{
				return new TimeOnly(timeOnly.Ticks - remainder);
			}
			return timeOnly;
		}

		/// <summary>
		/// Convert <paramref name="timeOnly"/> into the <typeparamref name="TParameter"/> which will be set on the parameter
		/// </summary>
		/// <param name="timeOnly"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		protected abstract TParameter GetParameterValueToSet(TimeOnly timeOnly, ISessionImplementor session);

		///<inheritdoc/>
		public override void Set(DbCommand st, object value, int index, ISessionImplementor session)
		{
			st.Parameters[index].Value = GetParameterValueToSet(AdjustTimeOnly((TimeOnly) value), session);
		}

		///<inheritdoc/>
		public override object Get(DbDataReader rs, int index, ISessionImplementor session)
		{
			try
			{
				return AdjustTimeOnly(GetTimeOnlyFromReader(rs, index, session));
			}
			catch (Exception ex) when (ex is not FormatException)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
			}
		}

		///<inheritdoc/>
		public override object Get(DbDataReader rs, string name, ISessionImplementor session) => Get(rs, rs.GetOrdinal(name), session);

		/// <summary>
		/// Get the <see cref="TimeOnly"/> value from the <see cref="DbDataReader"/> at index <paramref name="index"/>
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="index"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		protected abstract TimeOnly GetTimeOnlyFromReader(DbDataReader rs, int index, ISessionImplementor session);

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

			var time1 = (TimeOnly) x;
			var time2 = (TimeOnly) y;

			if (time1.Hour == time2.Hour &&
			   time1.Minute == time2.Minute &&
			   time1.Second == time2.Second)
			{
				return (!_fractionalSecondsPrecision.HasValue && time1 == time2) ||
					_fractionalSecondsPrecision == 0 ||
					AdjustTimeOnly(time1) == AdjustTimeOnly(time2);
			}

			return false;
		}

		public override int GetHashCode(object x)
		{
			return AdjustTimeOnly((TimeOnly) x).GetHashCode();
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
			return ((TimeOnly) val).ToString("T");
		}

		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version.")]
		public override object FromStringValue(string xml)
		{
			return TimeOnly.Parse(xml);
		}
	}
}
#endif
