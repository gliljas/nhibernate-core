using System;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;

namespace NHibernate.Test.TypesTest
{
	public abstract class TimeOrDateTypeFixtureBase<TTestClass,TTestType> : TestCase
	{
		protected virtual long DateAccuracyInTicks => Math.Max(TimeSpan.TicksPerMillisecond, Dialect.TimestampResolutionInTicks);
		protected byte ScaleFromDateAccuracyInTicks => (byte) Math.Floor(Math.Log10(TimeSpan.TicksPerSecond) - Math.Log10(DateAccuracyInTicks));

		protected override void AddMappings(Configuration configuration)
		{
			var mapper = new ModelMapper();
			
			AddMappingsToModelMapper(mapper);

			var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
			configuration.AddMapping(mapping);
		}

		protected override string[] Mappings => [];

		protected virtual void AddMappingsToModelMapper(ModelMapper mapper)
		{
		}

		protected override void OnTearDown()
		{
			using var s = OpenSession();
			using var t = s.BeginTransaction();
			s.Query<TTestClass>().Delete();
			t.Commit();
		}
	}
}
