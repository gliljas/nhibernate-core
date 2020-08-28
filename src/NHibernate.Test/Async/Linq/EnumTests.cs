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
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture(typeof(EnumType<TestEnum>),"0")]
	[TestFixture(typeof(EnumStringType<TestEnum>), "'Unspecified'")]
	[TestFixture(typeof(EnumAnsiStringType<TestEnum>), "'Unspecified'")]
	public class EnumTestsAsync : TestCaseMappingByCode
	{
		private IType _enumType;
		private string _unspecifiedValue;

		public EnumTestsAsync(System.Type enumType, string unspecifiedValue)
		{
			_enumType = (IType) Activator.CreateInstance(enumType);
			_unspecifiedValue = unspecifiedValue;
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<EnumEntity>(
				rc =>
				{
					rc.Table("EnumEntity");
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Property(x => x.Enum, m => m.Type(_enumType));
					rc.Property(x => x.NullableEnum, m =>
					{
						m.Type(_enumType);
						m.Formula($"(case when Enum = {_unspecifiedValue} then null else Enum end)");
					});
					rc.ManyToOne(x => x.Other, m => m.Cascade(Mapping.ByCode.Cascade.All));
				});


			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				session.Save(new EnumEntity { Enum = TestEnum.Unspecified });
				session.Save(new EnumEntity { Enum = TestEnum.Small });
				session.Save(new EnumEntity { Enum = TestEnum.Small });
				session.Save(new EnumEntity { Enum = TestEnum.Medium });
				session.Save(new EnumEntity { Enum = TestEnum.Medium });
				session.Save(new EnumEntity { Enum = TestEnum.Medium });
				session.Save(new EnumEntity { Enum = TestEnum.Large });
				session.Save(new EnumEntity { Enum = TestEnum.Large });
				session.Save(new EnumEntity { Enum = TestEnum.Large });
				session.Save(new EnumEntity { Enum = TestEnum.Large });
				trans.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task CanQueryOnEnum_Large_4Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Large, 4));
		}

		[Test]
		public async Task CanQueryOnEnum_Medium_3Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Medium, 3));
		}

		[Test]
		public async Task CanQueryOnEnum_Small_2Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Small, 2));
		}

		[Test]
		public async Task CanQueryOnEnum_Unspecified_1Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Unspecified, 1));
		}

		private async Task CanQueryOnEnumAsync(TestEnum type, int expectedCount, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var query = await (session.Query<EnumEntity>().Where(x => x.Enum == type).ToListAsync(cancellationToken));

				Assert.AreEqual(expectedCount, query.Count);
			}
		}

		[Test]
		public async Task CanQueryWithContainsOnTestEnum_Small_1Async()
		{
			var values = new[] { TestEnum.Small, TestEnum.Medium };
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var query = await (session.Query<EnumEntity>().Where(x => values.Contains(x.Enum)).ToListAsync());

				Assert.AreEqual(5, query.Count);
			}
		}

		[Test]
		public async Task ConditionalNavigationPropertyAsync()
		{
			TestEnum? type = null;
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var entities = session.Query<EnumEntity>();
				await (entities.Where(o => o.Enum == TestEnum.Large).ToListAsync());
				await (entities.Where(o => TestEnum.Large != o.Enum).ToListAsync());
				await (entities.Where(o => (o.NullableEnum ?? TestEnum.Large) == TestEnum.Medium).ToListAsync());
				await (entities.Where(o => ((o.NullableEnum ?? type) ?? o.Enum) == TestEnum.Medium).ToListAsync());

				await (entities.Where(o => (o.NullableEnum.HasValue ? o.Enum : TestEnum.Unspecified) == TestEnum.Medium).ToListAsync());
				await (entities.Where(o => (o.Enum != TestEnum.Large
										? (o.NullableEnum.HasValue ? o.Enum : TestEnum.Unspecified)
										: TestEnum.Small) == TestEnum.Medium).ToListAsync());

				await (entities.Where(o => (o.Enum == TestEnum.Large ? o.Other : o.Other).Name == "test").ToListAsync());
			}
		}

		[Test]
		public async Task CanQueryComplexExpressionOnTestEnumAsync()
		{
			var type = TestEnum.Unspecified;
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var entities = session.Query<EnumEntity>();

				var query = await ((from user in entities
							 where (user.NullableEnum == TestEnum.Large
									   ? TestEnum.Medium
									   : user.NullableEnum ?? user.Enum
								   ) == type
							 select new
							 {
								 user,
								 simple = user.Enum,
								 condition = user.Enum == TestEnum.Large ? TestEnum.Medium : user.Enum,
								 coalesce = user.NullableEnum ?? TestEnum.Medium
							 }).ToListAsync());

				Assert.That(query.Count, Is.EqualTo(1));
			}
		}

		public class EnumEntity
		{
			public virtual int Id { get; set; }
			public virtual string Name { get; set; }

			public virtual TestEnum Enum { get; set; }
			public virtual TestEnum? NullableEnum { get; set; }

			public virtual EnumEntity Other { get; set; }
		}

		public enum TestEnum
		{
			Unspecified,
			Small,
			Medium,
			Large
		}

		[Serializable]
		public class EnumAnsiStringType<T> : EnumStringType
		{
			private readonly string typeName;

			public EnumAnsiStringType()
				: base(typeof(T))
			{
				System.Type type = GetType();
				typeName = type.FullName + ", " + type.Assembly.GetName().Name;
			}

			public override string Name
			{
				get { return typeName; }
			}

			public override SqlType SqlType => SqlTypeFactory.GetAnsiString(255);
		}
	}
}
