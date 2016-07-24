#if NET_4_5
using NUnit.Framework;
using NHibernate.Mapping.ByCode;
using NHibernate.Cfg;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH3016
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync
	{
		[Test]
		public void ShouldAllowMappingComponentAsIdWithNestedClass()
		{
			var cfg = new Configuration();
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc => rc.ComponentAsId(entity => entity.Id, cid => cid.Property(p => p.Id)));
			var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
			cfg.AddDeserializedMapping(mapping, "TestDomain");
		}
	}
}
#endif
