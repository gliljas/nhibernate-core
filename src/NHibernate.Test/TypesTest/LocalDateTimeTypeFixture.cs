using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	/// <summary>
	/// The Unit Tests for the LocalDateTimeType.
	/// </summary>
	[TestFixture]
	public class LocalDateTimeTypeFixture : AbstractDateTimeTypeFixture<LocalDateTimeType>
	{
		protected override LocalDateTimeType Type => NHibernateUtil.LocalDateTime;
	}

	[TestFixture]
	public class LocalDateTimeTypeWithScaleFixture : AbstractDateTimeTypeWithScaleFixture<LocalDateTimeType>
	{
		protected override LocalDateTimeType Type => (LocalDateTimeType) TypeFactory.GetLocalDateTimeType(ScaleFromDateAccuracyInTicks);
	}

	[TestFixture]
	public class LocalDateTimeNoMsTypeFixture : AbstractDateTimeNoMsTypeFixture<LocalDateTimeNoMsType>
	{
		protected override LocalDateTimeNoMsType Type => NHibernateUtil.LocalDateTimeNoMs;
	}
}
