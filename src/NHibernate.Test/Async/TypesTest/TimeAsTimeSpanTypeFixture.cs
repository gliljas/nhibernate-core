﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.TypesTest
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <summary>
	/// Summary description for TimeAsTimeSpanTypeFixture.
	/// </summary>
	[TestFixture]
	public class TimeAsTimeSpanTypeFixtureAsync
	{
		[Test]
		public async Task NextAsync()
		{
			var type = (TimeAsTimeSpanType) NHibernateUtil.TimeAsTimeSpan;
			object current = new TimeSpan(DateTime.Now.Ticks - 5);
			object next = await (type.NextAsync(current, null, CancellationToken.None));

			Assert.IsTrue(next is TimeSpan, "Next should be TimeSpan");
			Assert.IsTrue((TimeSpan) next > (TimeSpan) current,
			              "next should be greater than current (could be equal depending on how quickly this occurs)");
		}

		[Test]
		public async Task SeedAsync()
		{
			var type = (TimeAsTimeSpanType) NHibernateUtil.TimeAsTimeSpan;
			Assert.IsTrue(await (type.SeedAsync(null, CancellationToken.None)) is TimeSpan, "seed should be TimeSpan");
		}
	}

	[TestFixture]
	public class TimeSpanFixture2Async : TimeOrDateTypeFixtureBase<TimeAsTimeSpanClass, TimeAsTimeSpanType>
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
		public async Task SavingAndRetrievingAsync()
		{
			var ticks = DateTime.Parse("23:59:59").TimeOfDay;

			var entity = new TimeAsTimeSpanClass
			             	{
			             		TimeSpanValue = ticks
			             	};

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				await (s.SaveAsync(entity));
				await (tx.CommitAsync());
			}

			TimeAsTimeSpanClass entityReturned;

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				entityReturned = await (s.CreateQuery("from TimeAsTimeSpanClass").UniqueResultAsync<TimeAsTimeSpanClass>());
				
				Assert.AreEqual(ticks, entityReturned.TimeSpanValue);
				Assert.AreEqual(entityReturned.TimeSpanValue.Hours,ticks.Hours);
				Assert.AreEqual(entityReturned.TimeSpanValue.Minutes, ticks.Minutes);
				Assert.AreEqual(entityReturned.TimeSpanValue.Seconds, ticks.Seconds);
			}
		}

		[Test]
		public async Task LowerDigitsAreIgnoredAsync()
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
				await (s.SaveAsync(entity));
				id = entity.Id;
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var retrieved = await (s.LoadAsync<TimeAsTimeSpanClass>(id));
				Assert.That(retrieved.TimeSpanWithScale, Is.EqualTo(baseTime));
				await (t.CommitAsync());
			}
		}
	}
}
