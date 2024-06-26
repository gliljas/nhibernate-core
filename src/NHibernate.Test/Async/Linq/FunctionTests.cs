﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.DomainModel;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FunctionTestsAsync : LinqTestCase
	{
		[Test]
		public async Task LikeFunctionAsync()
		{
			var query = await ((from e in db.Employees
						 where NHibernate.Linq.SqlMethods.Like(e.FirstName, "Ma%et")
						 select e).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
			Assert.That(query[0].FirstName, Is.EqualTo("Margaret"));
		}

		[Test]
		public async Task LikeFunctionWithEscapeCharacterAsync()
		{
			using (var tx = session.BeginTransaction())
			{
				var employeeName = "Mar%aret";
				var escapeChar = '#';
				var employeeNameEscaped = employeeName.Replace("%", escapeChar + "%");

				//This entity will be flushed to the db, but rolled back when the test completes

				await (session.SaveAsync(new Employee { FirstName = employeeName, LastName = "LastName" }));
				await (session.FlushAsync());

				var query = await ((from e in db.Employees
				             where NHibernate.Linq.SqlMethods.Like(e.FirstName, employeeNameEscaped, escapeChar)
				             select e).ToListAsync());

				Assert.That(query.Count, Is.EqualTo(1));
				Assert.That(query[0].FirstName, Is.EqualTo(employeeName));

				Assert.ThrowsAsync<ArgumentException>(() =>
				{
					return (from e in db.Employees
					 where NHibernate.Linq.SqlMethods.Like(e.FirstName, employeeNameEscaped, e.FirstName.First())
					 select e).ToListAsync();
				});
				await (tx.RollbackAsync());
			}
		}

		private static class SqlMethods
		{
			public static bool Like(string expression, string pattern)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public async Task LikeFunctionUserDefinedAsync()
		{
			// Verify that any method named Like, in a class named SqlMethods, will be translated.

			// ReSharper disable RedundantNameQualifier
			// NOTE: Deliberately use full namespace for our SqlMethods class below, to reduce
			// risk of accidentally referencing NHibernate.Linq.SqlMethods.
			var query = await ((from e in db.Employees
						 where NHibernate.Test.Linq.FunctionTestsAsync.SqlMethods.Like(e.FirstName, "Ma%et")
						 select e).ToListAsync());
			// ReSharper restore RedundantNameQualifier

			Assert.That(query.Count, Is.EqualTo(1));
			Assert.That(query[0].FirstName, Is.EqualTo("Margaret"));
		}

		[Test]
		public async Task SubstringFunction2Async()
		{
			var query = await ((from e in db.Employees
				where e.FirstName.Substring(0, 2) == "An"
				select e).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
		}

		[Test]
		public async Task SubstringFunction1Async()
		{
			var query = await ((from e in db.Employees
				where e.FirstName.Substring(3) == "rew"
				select e).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
			Assert.That(query[0].FirstName, Is.EqualTo("Andrew"));
		}

		[Test]
		public async Task GetCharsFunctionAsync()
		{
			var query = await ((
				from e in db.Employees
				where e.FirstName[2] == 'e'
				select e
			).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(1));
			Assert.That(query[0].FirstName, Is.EqualTo("Steven"));
		}

		[Test]
		public async Task LeftFunctionAsync()
		{
			var query = await ((from e in db.Employees
						 where e.FirstName.Substring(0, 2) == "An"
						 select e.FirstName.Substring(3)).ToListAsync());

			Assert.That(query.Count, Is.EqualTo(2));
			Assert.That(query[0], Is.EqualTo("rew")); //Andrew
			Assert.That(query[1], Is.EqualTo("e")); //Anne
		}

		[Test]
		public async Task ReplaceFunctionAsync()
		{
			var suppliedName = "Anne";
			var query = from e in db.Employees
						where e.FirstName.StartsWith("An")
						select new
							{
								Before = e.FirstName,
								// This one call the standard string.Replace, not the extension. The linq registry handles it.
								AfterMethod = e.FirstName.Replace("An", "Zan"),
								AfterExtension = ExtensionMethods.Replace(e.FirstName, "An", "Zan"),
								AfterNamedExtension = e.FirstName.ReplaceExtension("An", "Zan"),
								AfterEvaluableExtension = e.FirstName.ReplaceWithEvaluation("An", "Zan"),
								AfterEvaluable2Extension = e.FirstName.ReplaceWithEvaluation2("An", "Zan"),
							BeforeConst = suppliedName,
								// This one call the standard string.Replace, not the extension. The linq registry handles it.
								AfterMethodConst = suppliedName.Replace("An", "Zan"),
								AfterExtensionConst = ExtensionMethods.Replace(suppliedName, "An", "Zan"),
								AfterNamedExtensionConst = suppliedName.ReplaceExtension("An", "Zan"),
								AfterEvaluableExtensionConst = suppliedName.ReplaceWithEvaluation("An", "Zan"),
								AfterEvaluable2ExtensionConst = suppliedName.ReplaceWithEvaluation2("An", "Zan")
						};
			var results = await (query.ToListAsync());
			var s = await (ObjectDumper.WriteAsync(results));

			foreach (var result in results)
			{
				var expectedDbResult = Regex.Replace(result.Before, "An", "Zan", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				Assert.That(result.AfterMethod, Is.EqualTo(expectedDbResult), $"Wrong {nameof(result.AfterMethod)} value");
				Assert.That(result.AfterExtension, Is.EqualTo(expectedDbResult), $"Wrong {nameof(result.AfterExtension)} value");
				Assert.That(result.AfterNamedExtension, Is.EqualTo(expectedDbResult), $"Wrong {nameof(result.AfterNamedExtension)} value");
				Assert.That(result.AfterEvaluableExtension, Is.EqualTo(expectedDbResult), $"Wrong {nameof(result.AfterEvaluableExtension)} value");
				Assert.That(result.AfterEvaluable2Extension, Is.EqualTo(expectedDbResult), $"Wrong {nameof(result.AfterEvaluable2Extension)} value");

				var expectedDbResultConst = Regex.Replace(result.BeforeConst, "An", "Zan", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				var expectedInMemoryResultConst = result.BeforeConst.Replace("An", "Zan");
				var expectedInMemoryExtensionResultConst = result.BeforeConst.ReplaceWithEvaluation("An", "Zan");
				Assert.That(result.AfterMethodConst, Is.EqualTo(expectedInMemoryResultConst), $"Wrong {nameof(result.AfterMethodConst)} value");
				Assert.That(result.AfterExtensionConst, Is.EqualTo(expectedDbResultConst), $"Wrong {nameof(result.AfterExtensionConst)} value");
				Assert.That(result.AfterNamedExtensionConst, Is.EqualTo(expectedDbResultConst), $"Wrong {nameof(result.AfterNamedExtensionConst)} value");
				Assert.That(result.AfterEvaluableExtensionConst, Is.EqualTo(expectedInMemoryExtensionResultConst), $"Wrong {nameof(result.AfterEvaluableExtensionConst)} value");
				Assert.That(result.AfterEvaluable2ExtensionConst, Is.EqualTo(expectedInMemoryExtensionResultConst), $"Wrong {nameof(result.AfterEvaluable2ExtensionConst)} value");
			}

			// Should cause ReplaceWithEvaluation to fail
			suppliedName = null;
			var failingQuery = from e in db.Employees
						where e.FirstName.StartsWith("An")
						select new
						{
							Before = e.FirstName,
							AfterEvaluableExtensionConst = suppliedName.ReplaceWithEvaluation("An", "Zan")
						};
			Assert.That(() => failingQuery.ToListAsync(), Throws.InstanceOf<HibernateException>().And.InnerException.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public async Task CharIndexFunctionAsync()
		{
			var raw = await ((from e in db.Employees select e.FirstName).ToListAsync());
			var expected = raw.Select(x => x.ToLower()).Where(x => x.IndexOf('a') == 0).ToList();

			var query = from e in db.Employees
						let lowerName = e.FirstName.ToLower()
						where lowerName.IndexOf('a') == 0
						select lowerName;
			var result = await (query.ToListAsync());

			Assert.That(result, Is.EqualTo(expected), $"Expected {string.Join("|", expected)} but was {string.Join("|", result)}");
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task CharIndexOffsetNegativeFunctionAsync()
		{
			var raw = await ((from e in db.Employees select e.FirstName).ToListAsync());
			var expected = raw.Select(x => x.ToLower()).Where(x => x.IndexOf('a', 2) == -1).ToList();

			var query = from e in db.Employees
						let lowerName = e.FirstName.ToLower()
						where lowerName.IndexOf('a', 2) == -1
						select lowerName;
			var result = await (query.ToListAsync());

			Assert.That(result, Is.EqualTo(expected), $"Expected {string.Join("|", expected)} but was {string.Join("|", result)}");
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task IndexOfFunctionExpressionAsync()
		{
			var raw = await ((from e in db.Employees select e.FirstName).ToListAsync());
			var expected = raw.Select(x => x.ToLower()).Where(x => x.IndexOf("an") == 0).ToList();

			var query = from e in db.Employees
						let lowerName = e.FirstName.ToLower()
						where lowerName.IndexOf("an") == 0
						select lowerName;
			var result = await (query.ToListAsync());

			Assert.That(result, Is.EqualTo(expected), $"Expected {string.Join("|", expected)} but was {string.Join("|", result)}");
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task IndexOfFunctionProjectionAsync()
		{
			var raw = await ((from e in db.Employees select e.FirstName).ToListAsync());
			var expected = raw.Select(x => x.ToLower()).Where(x => x.Contains("a")).Select(x => x.IndexOf("a", 1)).ToList();

			var query = from e in db.Employees
						let lowerName = e.FirstName.ToLower()
						where lowerName.Contains("a")
						select lowerName.IndexOf("a", 1);
			var result = await (query.ToListAsync());

			Assert.That(result, Is.EqualTo(expected), $"Expected {string.Join("|", expected)} but was {string.Join("|", result)}");
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task TwoFunctionExpressionAsync()
		{
			var query = from e in db.Employees
						where e.FirstName.IndexOf("A") == e.BirthDate.Value.Month 
						select e.FirstName;

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task ToStringFunctionAsync()
		{
			var query = from ol in db.OrderLines
						where ol.Quantity.ToString() == "4"
						select ol;

			Assert.AreEqual(55, await (query.CountAsync()));
		}

		[Test]
		public async Task ToStringWithContainsAsync()
		{
			var query = from ol in db.OrderLines
						where ol.Quantity.ToString().Contains("5")
						select ol;

			Assert.AreEqual(498, await (query.CountAsync()));
		}

		[Test]
		public async Task CoalesceAsync()
		{
			Assert.AreEqual(2, await (session.Query<AnotherEntity>().CountAsync(e => (e.Input ?? "hello") == "hello")));
		}

		[Test]
		public async Task TrimAsync()
		{
			using (session.BeginTransaction())
			{
				AnotherEntity ae1 = new AnotherEntity {Input = " hi "};
				AnotherEntity ae2 = new AnotherEntity {Input = "hi"};
				AnotherEntity ae3 = new AnotherEntity {Input = "heh"};
				await (session.SaveAsync(ae1));
				await (session.SaveAsync(ae2));
				await (session.SaveAsync(ae3));
				await (session.FlushAsync());

				Assert.AreEqual(2, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.Trim() == "hi")));
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimEnd() == " hi")));

				// Emulated trim does not support multiple trim characters, but for many databases it should work fine anyways.
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.Trim('h') == "e")));
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimStart('h') == "eh")));
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimEnd('h') == "he")));

				// Check when passed as array
				// (the single character parameter is a new overload in .netcoreapp2.0, but not net461 or .netstandard2.0).
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.Trim(new [] { 'h' }) == "e")));
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimStart(new[] { 'h' }) == "eh")));
				Assert.AreEqual(1, await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimEnd(new[] { 'h' }) == "he")));

				// Let it rollback to get rid of temporary changes.
			}
		}

		[Test]
		public async Task TrimInitialWhitespaceAsync()
		{
			using (session.BeginTransaction())
			{
				await (session.SaveAsync(new AnotherEntity {Input = " hi"}));
				await (session.SaveAsync(new AnotherEntity {Input = "hi"}));
				await (session.SaveAsync(new AnotherEntity {Input = "heh"}));
				await (session.FlushAsync());

				Assert.That(await (session.Query<AnotherEntity>().CountAsync(e => e.Input.TrimStart() == "hi")), Is.EqualTo(2));

				// Let it rollback to get rid of temporary changes.
			}
		}

		[Test]
		public async Task WhereStringEqualAsync()
		{
			var query = await ((from item in db.Users
						 where item.Name.Equals("ayende")
						 select item).ToListAsync());
			await (ObjectDumper.WriteAsync(query));
		}

		[Test, Description("NH-3367")]
		public async Task WhereStaticStringEqualAsync()
		{
			var query = await ((from item in db.Users
						 where string.Equals(item.Name, "ayende")
						 select item).ToListAsync());
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereIntEqualAsync()
		{
			var query = await ((from item in db.Users
						 where item.Id.Equals(-1)
						 select item).ToListAsync());

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereBoolConstantEqualAsync()
		{
			var query = from item in db.Role
						where item.IsActive.Equals(true)
						select item;
			
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereBoolConditionEqualsAsync()
		{
			var query = from item in db.Role
						where item.IsActive.Equals(item.Name != null)
						select item;
			
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereBoolParameterEqualAsync()
		{
			var query = from item in db.Role
						where item.IsActive.Equals(1 == 1)
						select item;
			
			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereBoolFuncEqualAsync()
		{
			Func<bool> f = () => 1 == 1;

			var query = from item in db.Role
						where item.IsActive.Equals(f())
						select item;

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereLongEqualAsync()
		{
			var query = from item in db.PatientRecords
						 where item.Id.Equals(-1)
						 select item;

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereDateTimeEqualAsync()
		{
			var query = from item in db.Users
						where item.RegisteredAt.Equals(DateTime.Today)
						select item;

			await (ObjectDumper.WriteAsync(query));
		}
		
		[Test]
		public async Task WhereGuidEqualAsync()
		{
			var query = from item in db.Shippers
						where item.Reference.Equals(Guid.Empty)
						select item;

			await (ObjectDumper.WriteAsync(query));
		}		

		[Test]
		public async Task WhereDoubleEqualAsync()
		{
			var query = from item in db.Animals
						where item.BodyWeight.Equals(-1)
						select item;

			await (ObjectDumper.WriteAsync(query));
		}	

		[Test]
		public async Task WhereDecimalEqualAsync()
		{
			var query = from item in db.OrderLines
						where item.Discount.Equals(-1)
						select item;

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereEnumEqualAsync()
		{
			var query = from item in db.PatientRecords
						where item.Gender.Equals(Gender.Female)
						select item;

			await (ObjectDumper.WriteAsync(query));

			query = from item in db.PatientRecords
					where item.Gender.Equals(item.Gender)
					select item;

			await (ObjectDumper.WriteAsync(query));
		}


		[Test]
		public async Task WhereObjectEqualAsync()
		{
			var query = from item in db.PatientRecords
						where ((object) item.Gender).Equals(Gender.Female)
						select item;

			await (ObjectDumper.WriteAsync(query));
		}

		[Test]
		public async Task WhereEquatableEqualAsync()
		{
			var query = from item in db.Shippers
			            where ((IEquatable<Guid>) item.Reference).Equals(Guid.Empty)
			            select item;

			await (ObjectDumper.WriteAsync(query));
		}
	}
}
