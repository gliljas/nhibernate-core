#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Type;

namespace NHibernate.Test.TypesTest
{
	public class TimeOnlyAsIntTypeFixture : AbstractTimeOnlyTypeFixture<TimeOnlyAsIntType>
	{
		protected override IEnumerable<Expression<Func<TimeOnly, object>>> PropertiesToTest => null;
	}
}
#endif
