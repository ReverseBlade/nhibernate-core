#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Dialect;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1612
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NativeSqlCollectionLoaderFixtureAsync : BugTestCaseAsync
	{
#region Tests - <return-join>
		[Test]
		public async Task LoadElementsWithWithSimpleHbmAliasInjectionAsync()
		{
			string[] routes = CreateRoutes();
			Country country = await (LoadCountryWithNativeSQLAsync(CreateCountry(routes), "LoadCountryRoutesWithSimpleHbmAliasInjection"));
			Assert.That(country, Is.Not.Null);
			Assert.That(country.Routes, Is.EquivalentTo(routes));
			await (CleanupAsync());
		}

		[Test]
		public async Task LoadElementsWithExplicitColumnMappingsAsync()
		{
			string[] routes = CreateRoutes();
			Country country = await (LoadCountryWithNativeSQLAsync(CreateCountry(routes), "LoadCountryRoutesWithCustomAliases"));
			Assert.That(country, Is.Not.Null);
			Assert.That(country.Routes, Is.EquivalentTo(routes));
			await (CleanupAsync());
		}

		[Test]
		public async Task LoadCompositeElementsWithWithSimpleHbmAliasInjectionAsync()
		{
			IDictionary<int, AreaStatistics> stats = CreateStatistics();
			Country country = await (LoadCountryWithNativeSQLAsync(CreateCountry(stats), "LoadAreaStatisticsWithSimpleHbmAliasInjection"));
			Assert.That(country, Is.Not.Null);
			Assert.That((ICollection)country.Statistics.Keys, Is.EquivalentTo((ICollection)stats.Keys), "Keys");
			Assert.That((ICollection)country.Statistics.Values, Is.EquivalentTo((ICollection)stats.Values), "Elements");
			await (CleanupWithPersonsAsync());
		}

		[Test]
		public async Task LoadCompositeElementsWithWithComplexHbmAliasInjectionAsync()
		{
			IDictionary<int, AreaStatistics> stats = CreateStatistics();
			Country country = await (LoadCountryWithNativeSQLAsync(CreateCountry(stats), "LoadAreaStatisticsWithComplexHbmAliasInjection"));
			Assert.That(country, Is.Not.Null);
			Assert.That((ICollection)country.Statistics.Keys, Is.EquivalentTo((ICollection)stats.Keys), "Keys");
			Assert.That((ICollection)country.Statistics.Values, Is.EquivalentTo((ICollection)stats.Values), "Elements");
			await (CleanupWithPersonsAsync());
		}

		[Test]
		public async Task LoadCompositeElementsWithWithCustomAliasesAsync()
		{
			IDictionary<int, AreaStatistics> stats = CreateStatistics();
			Country country = await (LoadCountryWithNativeSQLAsync(CreateCountry(stats), "LoadAreaStatisticsWithCustomAliases"));
			Assert.That(country, Is.Not.Null);
			Assert.That((ICollection)country.Statistics.Keys, Is.EquivalentTo((ICollection)stats.Keys), "Keys");
			Assert.That((ICollection)country.Statistics.Values, Is.EquivalentTo((ICollection)stats.Values), "Elements");
			await (CleanupWithPersonsAsync());
		}

		[Test]
		public async Task LoadEntitiesWithWithSimpleHbmAliasInjectionAsync()
		{
			City[] cities = CreateCities();
			Country country = CreateCountry(cities);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var c = await (session.GetNamedQuery("LoadCountryCitiesWithSimpleHbmAliasInjection").SetString("country_code", country.Code).UniqueResultAsync<Country>());
				Assert.That(c, Is.Not.Null);
				Assert.That(c.Cities, Is.EquivalentTo(cities));
			}

			await (CleanupWithCitiesAsync());
		}

		[Test]
		public async Task LoadEntitiesWithComplexHbmAliasInjectionAsync()
		{
			City[] cities = CreateCities();
			Country country = CreateCountry(cities);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var c = await (session.GetNamedQuery("LoadCountryCitiesWithComplexHbmAliasInjection").SetString("country_code", country.Code).UniqueResultAsync<Country>());
				Assert.That(c, Is.Not.Null);
				Assert.That(c.Cities, Is.EquivalentTo(cities));
			}

			await (CleanupWithCitiesAsync());
		}

		[Test]
		public async Task LoadEntitiesWithExplicitColumnMappingsAsync()
		{
			City[] cities = CreateCities();
			Country country = CreateCountry(cities);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var c = await (session.GetNamedQuery("LoadCountryCitiesWithCustomAliases").SetString("country_code", country.Code).UniqueResultAsync<Country>());
				Assert.That(c, Is.Not.Null);
				Assert.That(c.Cities, Is.EquivalentTo(cities));
			}

			await (CleanupWithCitiesAsync());
		}

		[Test]
		public async Task NativeQueryWithUnresolvedHbmAliasInjectionAsync()
		{
			IDictionary<int, AreaStatistics> stats = CreateStatistics();
			try
			{
				await (LoadCountryWithNativeSQLAsync(CreateCountry(stats), "LoadAreaStatisticsWithFaultyHbmAliasInjection"));
				Assert.Fail("Expected exception");
			}
			catch (QueryException)
			{
			// ok
			}
			finally
			{
				await (CleanupWithPersonsAsync());
			}
		}

		private async Task<Country> LoadCountryWithNativeSQLAsync(Country country, string queryName)
		{
			// Ensure country is saved and session cache is empty to force from now on the reload of all 
			// persistence objects from the database.
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.SaveAsync(country));
					await (tx.CommitAsync());
				}
			}

			using (ISession session = OpenSession())
			{
				return await (session.GetNamedQuery(queryName).SetString("country_code", country.Code).UniqueResultAsync<Country>());
			}
		}

#endregion
#region Tests - <load-collection>
		[Test]
		public async Task LoadElementCollectionWithCustomLoaderAsync()
		{
			string[] routes = CreateRoutes();
			Country country = CreateCountry(routes);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var c = await (session.GetAsync<Country>(country.Code));
				Assert.That(c, Is.Not.Null, "country");
				Assert.That(c.Routes, Is.EquivalentTo(routes), "country.Routes");
			}

			await (CleanupAsync());
		}

		[Test]
		public async Task LoadCompositeElementCollectionWithCustomLoaderAsync()
		{
			IDictionary<int, AreaStatistics> stats = CreateStatistics();
			Country country = CreateCountry(stats);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var a = await (session.GetAsync<Area>(country.Code));
				Assert.That(a, Is.Not.Null, "area");
				Assert.That((ICollection)a.Statistics.Keys, Is.EquivalentTo((ICollection)stats.Keys), "area.Keys");
				Assert.That((ICollection)a.Statistics.Values, Is.EquivalentTo((ICollection)stats.Values), "area.Elements");
			}

			await (CleanupWithPersonsAsync());
		}

		[Test]
		public async Task LoadEntityCollectionWithCustomLoaderAsync()
		{
			City[] cities = CreateCities();
			Country country = CreateCountry(cities);
			await (SaveAsync(country));
			using (ISession session = OpenSession())
			{
				var c = await (session.GetAsync<Country>(country.Code));
				Assert.That(c, Is.Not.Null, "country");
				Assert.That(c.Cities, Is.EquivalentTo(cities), "country.Cities");
			}

			await (CleanupWithCitiesAsync());
		}

		private async Task SaveAsync<TArea>(TArea area)where TArea : Area
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.SaveAsync(area));
					await (tx.CommitAsync());
				}
			}
		}

#endregion
#region Tests - corner cases to verify backwards compatibility of NH-1612 patch
		[Test]
		public async Task NativeUpdateQueryWithoutResultsAsync()
		{
			if (!(Dialect is MsSql2000Dialect))
			{
				Assert.Ignore("This does not apply to {0}", Dialect);
			}

			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.GetNamedQuery("UpdateQueryWithoutResults").ExecuteUpdateAsync());
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task NativeScalarQueryWithoutResultsAsync()
		{
			if (!(Dialect is MsSql2000Dialect))
			{
				Assert.Ignore("This does not apply to {0}", Dialect);
			}

			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					// Native SQL Query outcome is not validated against <return-*> 
					// resultset declarations.
					await (session.GetNamedQuery("ScalarQueryWithDefinedResultsetButNoResults").ExecuteUpdateAsync());
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task NativeScalarQueryWithUndefinedResultsetAsync()
		{
			if (!(Dialect is MsSql2000Dialect))
			{
				Assert.Ignore("This does not apply to {0}", Dialect);
			}

			using (ISession session = OpenSession())
			{
				using (session.BeginTransaction())
				{
					// Native SQL Query outcome is not validated against <return-*> 
					// resultset declarations.
					var result = await (session.GetNamedQuery("ScalarQueryWithUndefinedResultset").UniqueResultAsync<int>());
					Assert.That(result, Is.EqualTo(1));
				}
			}
		}

		[Test]
		public async Task NativeScalarQueryWithDefinedResultsetAsync()
		{
			if (!(Dialect is MsSql2000Dialect))
			{
				Assert.Ignore("This does not apply to {0}", Dialect);
			}

			using (ISession session = OpenSession())
			{
				using (session.BeginTransaction())
				{
					// Native SQL Query outcome is not validated against <return-*> 
					// resultset declarations.
					var result = await (session.GetNamedQuery("ScalarQueryWithDefinedResultset").UniqueResultAsync<int>());
					Assert.That(result, Is.EqualTo(2));
				}
			}
		}

#endregion
#region cleanup
		private async Task CleanupAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Country"));
					await (tx.CommitAsync());
				}
			}
		}

		private async Task CleanupWithPersonsAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Person"));
					await (session.DeleteAsync("from Country"));
					await (tx.CommitAsync());
				}
			}
		}

		private async Task CleanupWithCitiesAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from City"));
					await (session.DeleteAsync("from Country"));
					await (tx.CommitAsync());
				}
			}
		}

#endregion
#region Factory methods
		private static Country CreateCountry()
		{
			const string COUNTRY_CODE = "WL";
			const string COUNTRY_NAME = "Wonderland";
			return new Country(COUNTRY_CODE, COUNTRY_NAME);
		}

		private static Country CreateCountry(params string[] routes)
		{
			Country country = CreateCountry();
			foreach (var route in routes)
			{
				country.Routes.Add(route);
			}

			return country;
		}

		private static Country CreateCountry(params City[] cities)
		{
			Country country = CreateCountry();
			foreach (var city in cities)
			{
				city.SetParent(country);
			}

			return country;
		}

		private static Country CreateCountry(IDictionary<int, AreaStatistics> statistics)
		{
			Country country = CreateCountry();
			foreach (var pair in statistics)
			{
				country.Statistics.Add(pair);
			}

			return country;
		}

		private static string[] CreateRoutes()
		{
			return new[]{"Yellow Road", "Muddy Path"};
		}

		private static City[] CreateCities()
		{
			return new[]{new City("EMR", "Emerald City"), new City("GLD", "Golden Town"), new City("NTH", "North End")};
		}

		private static IDictionary<int, AreaStatistics> CreateStatistics()
		{
			var archimedes = new Person("Archimedes");
			var archibald = new Person("Archibald");
			var amy = new Person("Amy");
			return new Dictionary<int, AreaStatistics>{{1850, new AreaStatistics{CitizenCount = 10000, GDP = new MonetaryValue("USD", 20000), Reporter = archimedes}}, {1900, new AreaStatistics{CitizenCount = 20000, GDP = new MonetaryValue("USD", 50000), Reporter = archibald}}, {1950, new AreaStatistics{CitizenCount = 40000, GDP = new MonetaryValue("USD", 125000)}}, {2000, new AreaStatistics{CitizenCount = 80000, GDP = new MonetaryValue("USD", 500000), Reporter = amy}}, };
		}
#endregion
	}
}
#endif
