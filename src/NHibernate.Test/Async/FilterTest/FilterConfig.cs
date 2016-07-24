#if NET_4_5
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Type;
using NUnit.Framework;
using System.Threading.Tasks;
using Exception = System.Exception;
using NHibernate.Util;

namespace NHibernate.Test.FilterTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FilterConfigAsync
	{
		private string mappingCfg = "NHibernate.Test.FilterTest.FilterMapping.hbm.xml";
		[Test]
		public void FilterDefinitionsLoadedCorrectly()
		{
			Configuration cfg = new Configuration();
			cfg.AddResource(mappingCfg, this.GetType().Assembly);
			Assert.AreEqual(cfg.FilterDefinitions.Count, 2);
			Assert.IsTrue(cfg.FilterDefinitions.ContainsKey("LiveFilter"));
			FilterDefinition f = cfg.FilterDefinitions["LiveFilter"];
			Assert.AreEqual(f.ParameterTypes.Count, 1);
			BooleanType t = f.ParameterTypes["LiveParam"] as BooleanType;
			Assert.IsNotNull(t); //ensure that the parameter is the correct type. 
		}

		[Test]
		public void FiltersLoaded()
		{
			Configuration cfg = new Configuration();
			cfg.AddResource(mappingCfg, this.GetType().Assembly);
			ISessionFactory factory = cfg.BuildSessionFactory();
			ISession session = factory.OpenSession();
			IFilter filter = session.EnableFilter("LiveFilter");
			Assert.AreEqual(filter.FilterDefinition.FilterName, "LiveFilter");
			filter.SetParameter("LiveParam", true);
			filter.Validate(); // make sure that everything is set up right. 
			IFilter filter2 = session.EnableFilter("LiveFilter2");
			filter2.SetParameter("LiveParam2", "somename");
			filter2.Validate();
		}

		[Test]
		public Task TestFilterThrowsWithNoParameterSetAsync()
		{
			try
			{
				Configuration cfg = new Configuration();
				cfg.AddResource(mappingCfg, this.GetType().Assembly);
				ISessionFactory factory = cfg.BuildSessionFactory();
				ISession session = factory.OpenSession();
				IFilter filter = session.EnableFilter("LiveFilter");
				Assert.Throws<HibernateException>(() => filter.Validate());
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}
	}
}
#endif
