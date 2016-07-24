#if NET_4_5
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH3727
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ByCodeFixtureAsync : TestCaseMappingByCodeAsync
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id);
			}

			);
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		[Test]
		public async Task QueryOverWithSubqueryProjectionCanBeExecutedMoreThanOnceAsync()
		{
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					const int parameter1 = 111;
					var countSubquery = QueryOver.Of<Entity>().Where(x => x.Id == parameter1) //any condition which makes output SQL has parameter
					.Select(Projections.RowCount());
					var originalQueryOver = session.QueryOver<Entity>().SelectList(l => l.Select(x => x.Id).SelectSubQuery(countSubquery)).TransformUsing(Transformers.ToList);
					var objects = await (originalQueryOver.ListAsync<object>());
					Assert.DoesNotThrowAsync(async () => await (originalQueryOver.ListAsync<object>()), "Second try to execute QueryOver thrown exception.");
				}
		}

		[Test]
		public async Task ClonedQueryOverExecutionMakesOriginalQueryOverNotWorkingAsync()
		{
			// Projections are copied by clone operation. 
			// SubqueryProjection use SubqueryExpression which holds CriteriaQueryTranslator (class SubqueryExpression { private CriteriaQueryTranslator innerQuery; })
			// So given CriteriaQueryTranslator is used twice. 
			// Since CriteriaQueryTranslator has CollectedParameters collection, second execution of the Criteria does not fit SqlCommand parameters.
			using (ISession session = OpenSession())
				using (session.BeginTransaction())
				{
					const int parameter1 = 111;
					var countSubquery = QueryOver.Of<Entity>().Where(x => x.Id == parameter1) //any condition which makes output SQL has parameter
					.Select(Projections.RowCount());
					var originalQueryOver = session.QueryOver<Entity>()//.Where(x => x.Id == parameter2)
					.SelectList(l => l.Select(x => x.Id).SelectSubQuery(countSubquery)).TransformUsing(Transformers.ToList);
					var clonedQueryOver = originalQueryOver.Clone();
					await (clonedQueryOver.ListAsync<object>());
					Assert.DoesNotThrowAsync(async () => await (originalQueryOver.ListAsync<object>()), "Cloned QueryOver execution caused source QueryOver throw exception when executed.");
				}
		}
	}
}
#endif
