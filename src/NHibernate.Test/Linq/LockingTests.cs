using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	public class LockingTests : LinqTestCase
	{
		[Test]
		public void CanSetLockMode()
		{
			using (var trans = session.BeginTransaction())
			{
				var entities = session.Query<AnotherEntity>().OrderBy(x=>x.Id).Take(1).LockMode(LockMode.Upgrade).ToList();
				Assert.AreEqual(
					LockMode.Upgrade,
					session.GetCurrentLockMode(entities.First()));

				entities = session.Query<AnotherEntity>().OrderBy(x => x.Id).Skip(1).Take(1).LockMode(LockMode.UpgradeNoWait).ToList();
				Assert.AreEqual(
					LockMode.UpgradeNoWait,
					session.GetCurrentLockMode(entities.First()));
			}
		}
	}
}
