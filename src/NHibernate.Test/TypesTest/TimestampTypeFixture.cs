using System;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	[TestFixture]
	[Obsolete]
	public class TimestampTypeFixture : AbstractDateTimeTypeFixture<TimestampType>
	{
		protected override TimestampType Type => NHibernateUtil.Timestamp;

		[Test]
		public void ObsoleteMessage()
		{
			using (var spy = new LogSpy(typeof(TypeFactory)))
			{
				TypeFactory.Basic(Type.GetType().FullName);
				var log = spy.GetWholeLog();
				Assert.That(
					log,
					Does.Contain("NHibernate.Type.TimestampType is obsolete. Please use DateTimeType instead.").IgnoreCase);
			}
		}
	}
}
