using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Linq;
using NHibernate.Linq.Visitors;
using NSubstitute;
using NUnit.Framework;
using Remotion.Linq;

namespace NHibernate.Test.Linq.Visitors
{
	[TestFixture]
	public class QueryOptionsExtractorTests
    {
		[Test]
	    public void NoOptionsShouldBeFound()
	    {
		    var orders = new List<Order>().AsQueryable();

		    var queryModel = CreateQueryModel(orders);

			var operators = QueryOptionsExtractor.ExtractOptions(queryModel);

		    Assert.That(operators.Count, Is.EqualTo(0));
		}

	    [Test]
	    public void OneOptionShouldBeFound()
	    {
		    var orders = new List<Order>().AsQueryable();

		    orders = orders.WithOptions(QueryCache.Enabled);

			var queryModel = CreateQueryModel(orders);

			var operators = QueryOptionsExtractor.ExtractOptions(queryModel);

		    Assert.That(operators.Count, Is.EqualTo(1));
		}

	    [Test]
	    public void FindsAllOptionsOnRootQuery()
	    {
			var orders = new List<Order>().AsQueryable();
			var orders2 = new List<Order>().AsQueryable();
		    var orderslines = new List<OrderLine>().AsQueryable();

		    orderslines = orderslines.WithOptions(QueryCache.Enabled.InRegion("Ignored"));

		    var result = orders
			    .WithOptions(QueryCache.Disabled)
			    .Join(orders2.WithOptions(QueryCache.Disabled), x => x.OrderId, x => x.OrderId, (x, y) => x)
			    .WithOptions(QueryCache.Disabled)
			    .Where(o => orderslines.Any(s => s.Order == o))
			    .WithOptions(QueryCache.Disabled)
			    .SelectMany(x => x.OrderLines)
			    .WithOptions(QueryCache.Disabled)
			    .Select(x => new {Id = x.Id})
			    .WithOptions(QueryCache.Enabled);



			var queryModel = CreateQueryModel(result);



			var operators = QueryOptionsExtractor.ExtractOptions(queryModel);


			Assert.That(operators.Count, Is.EqualTo(5));

		    var query = Substitute.For<IQuery>();

			operators.Last().Apply(query);

		    query.Received(1).SetCacheable(true);

			query.ClearReceivedCalls();

		    foreach (var queryOptionse in operators)
		    {
				queryOptionse.Apply(query);
			}
		    query.Received(4).SetCacheable(false);
		    query.Received(1).SetCacheable(true);
		    query.Received(0).SetCacheRegion(Arg.Any<string>());

		}

		private QueryModel CreateQueryModel<T>(IQueryable<T> queryable)
	    {
		    return NhRelinqQueryParser.Parse(NhRelinqQueryParser.PreTransform(queryable.Expression));
		}
    }
}
