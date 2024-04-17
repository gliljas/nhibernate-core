using System;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	public abstract class AbstractDateTimeTypeWithScaleFixture<TTestType> : AbstractDateTimeTypeFixture<TTestType> where TTestType : AbstractDateTimeType
	{
		// The timestamp rounding in seeding does not account scale.
		protected override bool RevisionCheck => false;

		//Use an accurency one digit less than the dialect's
		protected override long DateAccuracyInTicks => Math.Min(Dialect.TimestampResolutionInTicks * 10, TimeSpan.TicksPerSecond);


		[Test]
		public void LowerDigitsAreIgnored()
		{
			if (DateAccuracyInTicks == TimeSpan.TicksPerSecond)
				Assert.Ignore("The dialect doesn't support fractional seconds");

			var baseDate = new DateTime(2017, 10, 01, 17, 55, 24, GetTypeKind());
			var entity = new DateTimeClass
			{
				Id = AdditionalDateId,
				Value = baseDate.AddTicks(DateAccuracyInTicks / 10)
			};
			Assert.That(entity.Value, Is.Not.EqualTo(baseDate));

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Save(entity);
				t.Commit();
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var retrieved = s.Load<DateTimeClass>(AdditionalDateId);
				Assert.That(retrieved.Value, Is.EqualTo(baseDate));
				t.Commit();
			}
		}
	}
}
