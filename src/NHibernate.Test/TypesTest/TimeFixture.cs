using System;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	[TestFixture]
	public class TimeFixture : TimeOrDateTypeFixtureBase<TimeClass, TimeType>
	{
		protected override void AddMappingsToModelMapper(ModelMapper mapper)
		{
			mapper.Class<TimeClass>(m =>
			{
				m.Table("bc_time");
				m.Lazy(false);
				m.Id(p => p.Id, p => p.Generator(Generators.Native));
				m.Property(p => p.TimeValue,
					p => p.Type<TimeType>()
				);
				m.Property(p => p.TimeWithScale,
					p =>
					{
						p.Type<TimeType>();
						p.Scale(ScaleFromDateAccuracyInTicks);
					}
				);
			});
		}

		[Test]
		public void PropertiesHasExpectedType()
		{
			var classMetaData = Sfi.GetClassMetadata(typeof(TimeClass));
			Assert.That(
				classMetaData.GetPropertyType(nameof(TimeClass.TimeValue)),
				Is.EqualTo(NHibernateUtil.Time));
			Assert.That(
				classMetaData.GetPropertyType(nameof(TimeClass.TimeWithScale)),
				Is.EqualTo(TypeFactory.GetTimeType(ScaleFromDateAccuracyInTicks)));
		}

		[Test]
		public void SavingAndRetrieving()
		{
			var ticks = DateTime.Parse("23:59:59");

			var entity =
				new TimeClass
				{
					TimeValue = ticks
				};

			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.Save(entity);
				tx.Commit();
			}

			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var entityReturned = s.CreateQuery("from TimeClass").UniqueResult<TimeClass>();

				Assert.That(entityReturned.TimeValue.TimeOfDay, Is.EqualTo(ticks.TimeOfDay));
				Assert.That(ticks.Hour, Is.EqualTo(entityReturned.TimeValue.Hour));
				Assert.That(ticks.Minute, Is.EqualTo(entityReturned.TimeValue.Minute));
				Assert.That(ticks.Second, Is.EqualTo(entityReturned.TimeValue.Second));
				tx.Commit();
			}
		}

		[Test]
		public void LowerDigitsAreIgnored()
		{
			if (DateAccuracyInTicks == TimeSpan.TicksPerSecond)
				Assert.Ignore("The dialect doesn't support fractional seconds");

			var baseTime = new DateTime(1990, 01, 01, 17, 55, 24);
			var entity = new TimeClass
			{
				TimeWithScale = baseTime.Add(TimeSpan.FromTicks(DateAccuracyInTicks / 10))
			};
			Assert.That(entity.TimeWithScale, Is.Not.EqualTo(baseTime));

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
				var retrieved = s.Load<TimeClass>(id);
				Assert.That(retrieved.TimeWithScale.TimeOfDay, Is.EqualTo(baseTime.TimeOfDay));
				t.Commit();
			}
		}
	}
}
