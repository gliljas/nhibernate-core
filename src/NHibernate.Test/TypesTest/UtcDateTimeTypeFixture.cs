using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	/// <summary>
	/// The Unit Tests for the UtcDateTimeType.
	/// </summary>
	[TestFixture]
	public class UtcDateTimeTypeFixture : AbstractDateTimeTypeFixture<UtcDateTimeType>
	{
		protected override UtcDateTimeType Type => NHibernateUtil.UtcDateTime;
	}

	[TestFixture]
	public class UtcDateTimeTypeWithScaleFixture : AbstractDateTimeTypeWithScaleFixture<UtcDateTimeType>
	{
		protected override UtcDateTimeType Type => (UtcDateTimeType) TypeFactory.GetUtcDateTimeType(ScaleFromDateAccuracyInTicks);
	}

	[TestFixture]
	public class UtcDateTimeNoMsTypeFixture : AbstractDateTimeNoMsTypeFixture<UtcDateTimeNoMsType>
	{
		protected override UtcDateTimeNoMsType Type => NHibernateUtil.UtcDateTimeNoMs;
	}
}
