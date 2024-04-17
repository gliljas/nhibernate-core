using System;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	[TestFixture]
	public class UtcTicksTypeFixture : AbstractDateTimeTypeFixture<UtcTicksType>
	{
		protected override UtcTicksType Type => NHibernateUtil.UtcTicks;

		[Test]
		[TestCase("0")]
		[Obsolete]
		[Ignore("Ticks parse integer representations to date instead of date representations")]
		public override void FromStringValue_ParseValidValues(string timestampValue)
		{
		}

		[Ignore("Test relevant for datetime, not for ticks.")]
		public override void QueryUseExpectedSqlType()
		{
		}
	}
}
