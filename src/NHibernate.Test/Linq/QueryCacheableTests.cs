using System.Linq;
using NHibernate.Cfg;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	public class QueryCacheableTests : LinqTestCase
	{
		protected override void Configure(Configuration cfg)
		{
			cfg.SetProperty(Environment.UseQueryCache, "true");
			cfg.SetProperty(Environment.GenerateStatistics, "true");
			base.Configure(cfg);
		}

		[Test]
		public void QueryIsCacheable()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var x = (from c in db.Customers
					 select c)
				.WithOptions(QueryCache.Enabled)
				.ToList();

			var x2 = (from c in db.Customers
					  select c)
				.WithOptions(QueryCache.Enabled)
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(1), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void QueryIsCacheable2()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var x = (from c in db.Customers
					 select c)
				.WithOptions(QueryCache.Enabled)
				.ToList();

			var x2 = (from c in db.Customers
					  select c).ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(0), "Unexpected cache hit count");
		}

		[Test]
		public void QueryIsCacheable3()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var x = (from c in db.Customers.WithOptions(QueryCache.Enabled)
					 select c).ToList();

			var x2 = (from c in db.Customers
					  select c).ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(0), "Unexpected cache hit count");
		}

		[Test]
		public void QueryIsCacheableWithRegion()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var x = (from c in db.Customers
					 select c)
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.ToList();

			var x2 = (from c in db.Customers
					  select c)
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.ToList();

			var x3 = (from c in db.Customers
					  select c)
				.WithOptions(QueryCache.Enabled.InRegion("other"))
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(2), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void CacheableBeforeOtherClauses()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			db.Customers
				.WithOptions(QueryCache.Enabled)
				.Where(c => c.ContactName != c.CompanyName).Take(1).ToList();
			db.Customers.Where(c => c.ContactName != c.CompanyName).Take(1).ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(0), "Unexpected cache hit count");
		}

		[Test]
		public void CacheableRegionBeforeOtherClauses()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			db.Customers
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.Where(c => c.ContactName != c.CompanyName).Take(1)
				.ToList();
			db.Customers
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.Where(c => c.ContactName != c.CompanyName).Take(1)
				.ToList();
			db.Customers
				.WithOptions(QueryCache.Enabled.InRegion("other"))
				.Where(c => c.ContactName != c.CompanyName).Take(1)
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(2), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void CacheableRegionSwitched()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			db.Customers
				.Where(c => c.ContactName != c.CompanyName).Take(1)
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.ToList();
			db.Customers
				.Where(c => c.ContactName != c.CompanyName).Take(1)
				.WithOptions(QueryCache.Enabled.InRegion("test"))
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(1), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void GroupByQueryIsCacheable()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var c = db
				.Customers
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.WithOptions(QueryCache.Enabled)
				.ToList();

			c = db
				.Customers
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.ToList();

			c = db
				.Customers
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.WithOptions(QueryCache.Enabled)
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void GroupByQueryIsCacheable2()
		{
			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			var c = db
				.Customers
				.WithOptions(QueryCache.Enabled)
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.ToList();

			c = db
				.Customers
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.ToList();

			c = db
				.Customers
				.WithOptions(QueryCache.Enabled)
				.GroupBy(x => x.Address.Country)
				.Select(x => x.Key)
				.ToList();

			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(2), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(1), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}

		[Test]
		public void CanBeCombinedWithFetch()
		{
			//NH-2587
			//NH-3982 (GH-1372)

			Sfi.Statistics.Clear();
			Sfi.QueryCache.Clear();

			db.Customers
				.WithOptions(QueryCache.Enabled)
				.ToList();

			db.Orders
				.WithOptions(QueryCache.Enabled)
				.ToList();

			db.Customers
			   .WithOptions(QueryCache.Enabled)
				.Fetch(x => x.Orders)
				.ToList();

			db.Orders
				.WithOptions(QueryCache.Enabled)
				.Fetch(x => x.OrderLines)
				.ToList();

			var customer = db.Customers
				.WithOptions(QueryCache.Enabled)
				.Fetch(x => x.Address)
				.Where(x => x.CustomerId == "VINET")
				.SingleOrDefault();

			

			customer = db.Customers
			  .WithOptions(QueryCache.Enabled)
			  .Fetch(x => x.Address)
			  .Where(x => x.CustomerId == "VINET")
			  .SingleOrDefault();

			


			Assert.That(NHibernateUtil.IsInitialized(customer.Address), Is.True, "Expected the fetched Address to be initialized");
			Assert.That(Sfi.Statistics.QueryExecutionCount, Is.EqualTo(5), "Unexpected execution count");
			Assert.That(Sfi.Statistics.QueryCachePutCount, Is.EqualTo(5), "Unexpected cache put count");
			Assert.That(Sfi.Statistics.QueryCacheHitCount, Is.EqualTo(1), "Unexpected cache hit count");
		}
	}
}
