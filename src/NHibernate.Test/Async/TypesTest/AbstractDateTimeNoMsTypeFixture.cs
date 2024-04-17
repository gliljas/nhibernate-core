﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Type;

namespace NHibernate.Test.TypesTest
{
	using System.Threading.Tasks;
	public abstract class AbstractDateTimeNoMsTypeFixtureAsync<TTestType> : AbstractDateTimeTypeFixtureAsync<TTestType> where TTestType : AbstractDateTimeType
	{
		protected override bool RevisionCheck => false;
		protected override long DateAccuracyInTicks => TimeSpan.TicksPerSecond;

		protected override DateTime GetTestDate(DateTimeKind kind)
		{
			var date = base.GetTestDate(kind);
			return new DateTime(
				date.Year,
				date.Month,
				date.Day,
				date.Hour,
				date.Minute,
				date.Second,
				0,
				kind);
		}

		protected override DateTime GetSameDate(DateTime original)
		{
			var date = base.GetSameDate(original);
			return date.AddMilliseconds(date.Millisecond < 500 ? 500 : -500);
		}
	}
}
