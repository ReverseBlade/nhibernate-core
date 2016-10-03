﻿#if NET_4_5
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.MappingByCode.IntegrationTests.NH3280
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class OneToOneToInheritedPropertyAsync : TestCaseMappingByCodeAsync
	{
		private int _person1Id;
		private int _person2Id;
		private int _personDetailId;
		protected override async Task OnSetUpAsync()
		{
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					var person1 = new Person{FirstName = "Jack"};
					_person1Id = (int)await (session.SaveAsync(person1));
					var person2 = new Person{FirstName = "Robert"};
					_person2Id = (int)await (session.SaveAsync(person2));
					var personDetail = new PersonDetail{LastName = "Smith", Person = person1};
					_personDetailId = (int)await (session.SaveAsync(personDetail));
					await (tx.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from System.Object"));
					await (tx.CommitAsync());
				}
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<PersonDetail>(m =>
			{
				m.Id(t => t.PersonDetailId, a => a.Generator(Generators.Identity));
				m.Property(t => t.LastName, c =>
				{
					c.NotNullable(true);
					c.Length(32);
				}

				);
				m.ManyToOne(t => t.Person, c =>
				{
					c.Column("PersonId");
					c.Unique(true);
					c.NotNullable(false);
					c.NotFound(NotFoundMode.Ignore);
				}

				);
			}

			);
			mapper.Class<Person>(m =>
			{
				m.Id(t => t.PersonId, a => a.Generator(Generators.Identity));
				m.Property(t => t.FirstName, c =>
				{
					c.NotNullable(true);
					c.Length(32);
				}

				);
				m.OneToOne(t => t.PersonDetail, oo =>
				{
					oo.PropertyReference(typeof (PersonDetail).GetProperty("Person"));
					oo.Cascade(Mapping.ByCode.Cascade.All);
				}

				);
			}

			);
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		[Test]
		public async Task ShouldConfigureSessionCorrectlyAsync()
		{
			using (var session = OpenSession())
				using (session.BeginTransaction())
				{
					var person1 = await (session.GetAsync<Person>(_person1Id));
					var person2 = await (session.GetAsync<Person>(_person2Id));
					var personDetail = await (session.GetAsync<PersonDetail>(_personDetailId));
					Assert.IsNull(person2.PersonDetail);
					Assert.IsNotNull(person1.PersonDetail);
					Assert.AreEqual(person1.PersonDetail.LastName, personDetail.LastName);
					Assert.AreEqual(person1.FirstName, personDetail.Person.FirstName);
				}
		}
	}
}
#endif
