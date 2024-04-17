using System;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Type;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.TypesTest
{
	/// <summary>
	/// TestFixtures for the <see cref="DateTimeType"/>.
	/// </summary>
	[TestFixture]
	public class DateTimeTypeFixture : AbstractDateTimeTypeFixture<DateTimeType>
	{
		protected override DateTimeType Type => NHibernateUtil.DateTime;
	}
	/// <summary>
	/// TestFixtures for the <see cref="DateTimeType"/>.
	/// </summary>
	[TestFixture]
	public class DateTimeTypeWithScaleFixture : AbstractDateTimeTypeWithScaleFixture<DateTimeType>
	{
		protected override DateTimeType Type => (DateTimeType) TypeFactory.GetDateTimeType(ScaleFromDateAccuracyInTicks);
		protected override long DateAccuracyInTicks => Math.Max(TimeSpan.TicksPerMillisecond, base.DateAccuracyInTicks);

	}

	// Testing SQL Server 2008 with datetime in db instead of datetime2
	[TestFixture]
	public class DateTimeTypeMixedDateTimeFixture : DateTimeTypeFixture
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return base.AppliesTo(dialect) && dialect is MsSql2008Dialect;
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);

			configuration.SetProperty(Environment.SqlTypesKeepDateTime, "true");
		}

		/* Uncomment for testing how it fails if NHibernate uses DateTime2 while the db column is DateTime.
		 * It fails two thirds of times on update with an undue optimistic locking exception, due to the datetimes
		 * ending with 3.333ms (transmitted as 3) or 6.666ms (transmitted as 7), which does not match the datetime2
		 * sent by NHibernate for checking the revision has not changed. Demonstrated by UpdateUseExpectedSqlType.
		 * It furthermore causes undue optimistic failures if re-updating the same entity without loading it in-between,
		 * as demonstrated by ReadWrite.
		 * Another way to demonstrate this is to use a <version ...><column sql-type="datetime" ... /></version>
		 * mapping, without using SqlTypesKeepDateTime. This causes the column to be typed datetime in db but the
		 * parameters are still transmitted as datetime2 because this sql-type attribute is only used in DDL.
		 *
		protected override DebugSessionFactory BuildSessionFactory()
		{
			cfg.SetProperty(Environment.SqlTypesKeepDateTime, "false");
			try
			{
				return base.BuildSessionFactory();
			}
			finally
			{
				cfg.SetProperty(Environment.SqlTypesKeepDateTime, "true");
			}
		}
		//*/
	}

	/// <summary>
	/// TestFixtures for the <see cref="DateTimeNoMsType"/>.
	/// </summary>
	[TestFixture]
	public class DateTimeNoMsTypeFixture : AbstractDateTimeNoMsTypeFixture<DateTimeNoMsType>
	{
		protected override DateTimeNoMsType Type => NHibernateUtil.DateTimeNoMs;
		
	}
}
