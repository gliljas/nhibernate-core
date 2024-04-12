#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using NHibernate.Type;

namespace NHibernate.Test.TypesTest
{
	public class DateOnlyAsIntTypeFixture : GenericTypeFixtureBase<DateOnly, DateOnlyAsIntType>
	{
		protected override IReadOnlyList<DateOnly> TestValues =>
		[
			DateOnly.FromDateTime(Sfi.ConnectionProvider.Driver.MinDate),
			DateOnly.FromDateTime(DateTime.Now),
			DateOnly.MaxValue
		];
	}
}
#endif
