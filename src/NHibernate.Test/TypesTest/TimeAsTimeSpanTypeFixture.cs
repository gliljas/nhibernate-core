using System;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	/// <summary>
	/// Summary description for TimeAsTimeSpanTypeFixture.
	/// </summary>
	[TestFixture]
	public class TimeAsTimeSpanTypeFixture
	{
		[Test]
		public void Next()
		{
			var type = (TimeAsTimeSpanType) NHibernateUtil.TimeAsTimeSpan;
			object current = new TimeSpan(DateTime.Now.Ticks - 5);
			object next = type.Next(current, null);

			Assert.IsTrue(next is TimeSpan, "Next should be TimeSpan");
			Assert.IsTrue((TimeSpan) next > (TimeSpan) current,
			              "next should be greater than current (could be equal depending on how quickly this occurs)");
		}

		[Test]
		public void Seed()
		{
			var type = (TimeAsTimeSpanType) NHibernateUtil.TimeAsTimeSpan;
			Assert.IsTrue(type.Seed(null) is TimeSpan, "seed should be TimeSpan");
		}
	}

	[TestFixture]
	public class TimeSpanFixture2 : TimeOrDateTypeFixtureBase<TimeAsTimeSpanClass, TimeAsTimeSpanType>
	{
		protected override void AddMappingsToModelMapper(ModelMapper mapper)
		{
			mapper.Class<TimeAsTimeSpanClass>(m =>
			{
				m.Table("bc_timespan");
				m.Lazy(false);
				m.Id(p => p.Id, p => p.Generator(Generators.Native));
				m.Property(p => p.TimeSpanValue,
					p => p.Type<TimeAsTimeSpanType>()
				);
				m.Property(p => p.TimeSpanWithScale,
					p =>
					{
						p.Type<TimeAsTimeSpanType>();
						p.Scale(ScaleFromDateAccuracyInTicks);
					}
				);
			});
		}

		[Test]
		public void PropertiesHasExpectedType()
		{
			var classMetaData = Sfi.GetClassMetadata(typeof(TimeAsTimeSpanClass));
			Assert.That(
				classMetaData.GetPropertyType(nameof(TimeAsTimeSpanClass.TimeSpanValue)),
				Is.EqualTo(NHibernateUtil.TimeAsTimeSpan));
			Assert.That(
				classMetaData.GetPropertyType(nameof(TimeAsTimeSpanClass.TimeSpanWithScale)),
				Is.EqualTo(TypeFactory.GetTimeAsTimeSpanType(ScaleFromDateAccuracyInTicks)));
		}

		[Test]
		public void SavingAndRetrieving()
		{
			var ticks = DateTime.Parse("23:59:59").TimeOfDay;

			var entity = new TimeAsTimeSpanClass
			             	{
			             		TimeSpanValue = ticks
			             	};

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.Save(entity);
				tx.Commit();
			}

			TimeAsTimeSpanClass entityReturned;

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				entityReturned = s.CreateQuery("from TimeAsTimeSpanClass").UniqueResult<TimeAsTimeSpanClass>();
				
				Assert.AreEqual(ticks, entityReturned.TimeSpanValue);
				Assert.AreEqual(entityReturned.TimeSpanValue.Hours,ticks.Hours);
				Assert.AreEqual(entityReturned.TimeSpanValue.Minutes, ticks.Minutes);
				Assert.AreEqual(entityReturned.TimeSpanValue.Seconds, ticks.Seconds);
			}
		}

		[Test]
		public void LowerDigitsAreIgnored()
		{
			if (DateAccuracyInTicks == TimeSpan.TicksPerSecond)
				Assert.Ignore("The dialect doesn't support fractional seconds");

			var baseTime = new TimeSpan(0, 17, 55, 24);
			var entity = new TimeAsTimeSpanClass
			{
				TimeSpanWithScale = baseTime.Add(TimeSpan.FromTicks(DateAccuracyInTicks / 10))
			};
			Assert.That(entity.TimeSpanWithScale, Is.Not.EqualTo(baseTime));

			int id;
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Save(entity);
				id = entity.Id;
				t.Commit();
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var retrieved = s.Load<TimeAsTimeSpanClass>(id);
				Assert.That(retrieved.TimeSpanWithScale, Is.EqualTo(baseTime));
				t.Commit();
			}
		}
	}
}
