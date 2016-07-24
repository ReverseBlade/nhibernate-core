#if NET_4_5
using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Config;
using NHibernate.Cfg;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.FilterTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ConfigFixtureAsync
	{
		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private class ConfigurationStub : Configuration
		{
			public Queue<FilterSecondPassArgs> FiltersSecondPasses
			{
				get
				{
					return filtersSecondPasses;
				}
			}
		}

		private static ConfigurationStub GetConfiguration()
		{
			var result = new ConfigurationStub();
			if (TestConfigurationHelper.hibernateConfigFile != null)
				result.Configure(TestConfigurationHelper.hibernateConfigFile);
			return result;
		}

		[Test]
		[Description("Add a class with filters without condition should not Throw exceptions and add secondpass tasks.")]
		public Task AddClassWithFiltersAsync()
		{
			try
			{
				var cfg = GetConfiguration();
				Assert.DoesNotThrow(() => cfg.AddResource("NHibernate.Test.FilterTest.SimpleFiltered.hbm.xml", GetType().Assembly));
				Assert.That(cfg.FiltersSecondPasses.Count, Is.EqualTo(2));
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Add filters-def should change conditions of class filters")]
		public void AddFilterDefToClassWithFilters()
		{
			var cfg = GetConfiguration();
			cfg.AddResource("NHibernate.Test.FilterTest.SimpleFiltered.hbm.xml", GetType().Assembly);
			cfg.AddResource("NHibernate.Test.FilterTest.SimpleFilteredFiltersDefsOk.hbm.xml", GetType().Assembly);
			Assert.That(cfg.FilterDefinitions, Is.Not.Empty);
			cfg.BuildMappings();
			var pc = cfg.GetClassMapping(typeof (TestClass));
			foreach (var filterMap in pc.FilterMap)
			{
				Assert.That(filterMap.Value, Is.Not.Null & Is.Not.Empty, "filtername:" + filterMap.Key);
			}
		}

		[Test]
		[Description("Filter def without condition in both sides should throw exception")]
		public Task WrongFilterDefInClassAsync()
		{
			try
			{
				var cfg = GetConfiguration();
				var e = Assert.Throws<MappingException>(() => cfg.AddResource("NHibernate.Test.FilterTest.WrongFilterDefInClass.hbm.xml", GetType().Assembly));
				Assert.That(e.InnerException, Is.Not.Null);
				Assert.That(e.InnerException.Message, Is.StringStarting("no filter condition").IgnoreCase);
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Filter def without condition in both sides should throw exception even in secondpass")]
		public Task WrongFilterDefInClassSeconPassAsync()
		{
			try
			{
				const string wrongClassMap = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' 
				   default-lazy='false' 
				   assembly='NHibernate.Test' 
				   namespace='NHibernate.Test.FilterTest' >

	<class name='TestClass'>
		<id name='Id' column='id'>
			<generator class='assigned' />
		</id>
		<property name='Name'/>

		<property name='Live'/>
		<filter name='LiveFilter'/>
	</class>

</hibernate-mapping>";
				const string wrongFilterDef = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' >

	<filter-def name='LiveFilter'>
		<filter-param name='LiveParam' type='boolean'/>
	</filter-def>

</hibernate-mapping>";
				var cfg = GetConfiguration();
				cfg.AddXmlString(wrongClassMap);
				cfg.AddXmlString(wrongFilterDef);
				var e = Assert.Throws<MappingException>(cfg.BuildMappings);
				Assert.That(e.Message, Is.StringStarting("no filter condition").IgnoreCase);
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Add a class with filters without condition should Throw exceptions at secondpass.")]
		public Task AddClassWithFiltersWithoutFilterDefAsync()
		{
			try
			{
				var cfg = GetConfiguration();
				cfg.AddResource("NHibernate.Test.FilterTest.SimpleFiltered.hbm.xml", GetType().Assembly);
				var e = Assert.Throws<MappingException>(cfg.BuildMappings);
				Assert.That(e.Message, Is.StringStarting("filter-def for filter named"));
				Assert.That(e.Message, Is.StringContaining("was not found"));
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Class filter with condition does not add secondpass and add an invalid filter-def")]
		public void ClassNoSecondPass()
		{
			const string classMap = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' 
				   default-lazy='false' 
				   assembly='NHibernate.Test' 
				   namespace='NHibernate.Test.FilterTest' >

	<class name='TestClass'>
		<id name='Id' column='id'>
			<generator class='assigned' />
		</id>
		<property name='Name'/>

		<property name='Live'/>
		<filter name='LiveFilter' condition=':LiveParam = Live'/>
	</class>

</hibernate-mapping>";
			var cfg = GetConfiguration();
			cfg.AddXmlString(classMap);
			Assert.That(cfg.FiltersSecondPasses.Count, Is.EqualTo(0));
			Assert.That(cfg.FilterDefinitions.Keys, Has.Member("LiveFilter"));
			Assert.That(cfg.FilterDefinitions["LiveFilter"], Is.Null);
		}

		[Test]
		[Description("Writing the condition in both sides should not change the condition defined in the class.")]
		public void ClassConditionInBothSides()
		{
			const string classMap = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' 
				   default-lazy='false' 
				   assembly='NHibernate.Test' 
				   namespace='NHibernate.Test.FilterTest' >

	<class name='TestClass'>
		<id name='Id' column='id'>
			<generator class='assigned' />
		</id>
		<property name='Name'/>

		<property name='Live'/>
		<filter name='LiveFilter' condition='Live = 1'/>
	</class>

</hibernate-mapping>";
			const string filterDef = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' >

	<filter-def name='LiveFilter' condition=':LiveParam = Live'>
		<filter-param name='LiveParam' type='boolean'/>
	</filter-def>

</hibernate-mapping>";
			var cfg = GetConfiguration();
			cfg.AddXmlString(classMap);
			cfg.AddXmlString(filterDef);
			Assert.That(cfg.FiltersSecondPasses.Count, Is.EqualTo(0));
			Assert.That(cfg.FilterDefinitions.Keys, Has.Member("LiveFilter"));
			Assert.That(cfg.FilterDefinitions["LiveFilter"], Is.Not.Null);
			cfg.BuildMappings();
			Assert.That(cfg.FilterDefinitions.Count, Is.EqualTo(1));
			var pc = cfg.GetClassMapping(typeof (TestClass));
			Assert.That(pc.FilterMap["LiveFilter"], Is.EqualTo("Live = 1"));
		}

		[Test]
		[Description("Filter-def duplication should Throw exception")]
		public Task DuplicatedFilterDefAsync()
		{
			try
			{
				const string filterDef = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' >

	<filter-def name='LiveFilter'>
		<filter-param name='LiveParam' type='boolean'/>
	</filter-def>

	<filter-def name='LiveFilter'>
		<filter-param name='LiveParam' type='boolean'/>
	</filter-def>

</hibernate-mapping>";
				var cfg = GetConfiguration();
				var e = Assert.Throws<MappingException>(() => cfg.AddXmlString(filterDef));
				Assert.That(e.InnerException, Is.Not.Null);
				Assert.That(e.InnerException.Message, Is.StringContaining("Duplicated filter-def"));
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Add a filtered class with condition but without a filter-def should Throw exception")]
		public Task MissedFilterDefAsync()
		{
			try
			{
				const string classMap = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' 
				   default-lazy='false' 
				   assembly='NHibernate.Test' 
				   namespace='NHibernate.Test.FilterTest' >

	<class name='TestClass'>
		<id name='Id' column='id'>
			<generator class='assigned' />
		</id>
		<property name='Name'/>

		<property name='Live'/>
		<filter name='LiveFilter' condition='Live = 1'/>
	</class>

</hibernate-mapping>";
				var cfg = GetConfiguration();
				cfg.AddXmlString(classMap);
				var e = Assert.Throws<MappingException>(() => cfg.BuildSessionFactory());
				Assert.That(e.Message, Is.StringStarting("filter-def for filter named"));
				Assert.That(e.Message, Is.StringContaining("was not found"));
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		[Description("Filter-def without reference to it should Throw exception")]
		public void FilterDefWithoutReference()
		{
			const string filterDef = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' >

	<filter-def name='LiveFilter'>
		<filter-param name='LiveParam' type='boolean'/>
	</filter-def>

</hibernate-mapping>";
			var cfg = GetConfiguration();
			cfg.AddXmlString(filterDef);
			var memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
			try
			{
				cfg.BuildSessionFactory();
				var wholeLog = String.Join("\r\n", memoryAppender.GetEvents().Select(x => x.RenderedMessage).ToArray());
				Assert.That(wholeLog, Is.StringContaining("filter-def for filter named"));
				Assert.That(wholeLog, Is.StringContaining("was never used to filter classes nor collections."));
			}
			finally
			{
				LogManager.Shutdown();
			}
		}
	}
}
#endif
