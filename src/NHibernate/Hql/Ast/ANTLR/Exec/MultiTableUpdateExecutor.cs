using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Hql.Ast.ANTLR.Tree;
using NHibernate.Param;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Util;
using IQueryable = NHibernate.Persister.Entity.IQueryable;
using System.Threading.Tasks;

namespace NHibernate.Hql.Ast.ANTLR.Exec
{
	[CLSCompliant(false)]
	public class MultiTableUpdateExecutor : AbstractStatementExecutor
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof (MultiTableDeleteExecutor));
		private readonly IQueryable persister;
		private readonly SqlString idInsertSelect;
		private readonly SqlString[] updates;
		private readonly IParameterSpecification[][] hqlParameters;

		public MultiTableUpdateExecutor(IStatement statement) : base(statement, log)
		{
			if (!Factory.Dialect.SupportsTemporaryTables)
			{
				throw new HibernateException("cannot perform multi-table updates using dialect not supporting temp tables");
			}
			var updateStatement = (UpdateStatement) statement;

			FromElement fromElement = updateStatement.FromClause.GetFromElement();
			string bulkTargetAlias = fromElement.TableAlias;
			persister = fromElement.Queryable;

			idInsertSelect = GenerateIdInsertSelect(persister, bulkTargetAlias, updateStatement.WhereClause);
			log.Debug("Generated ID-INSERT-SELECT SQL (multi-table update) : " + idInsertSelect);

			string[] tableNames = persister.ConstraintOrderedTableNameClosure;
			string[][] columnNames = persister.ContraintOrderedTableKeyColumnClosure;

			string idSubselect = GenerateIdSubselect(persister);
			IList<AssignmentSpecification> assignmentSpecifications = Walker.AssignmentSpecifications;

			updates = new SqlString[tableNames.Length];
			hqlParameters = new IParameterSpecification[tableNames.Length][];
			for (int tableIndex = 0; tableIndex < tableNames.Length; tableIndex++)
			{
				bool affected = false;
				var parameterList = new List<IParameterSpecification>();
				SqlUpdateBuilder update =
					new SqlUpdateBuilder(Factory.Dialect, Factory).SetTableName(tableNames[tableIndex])
					.SetWhere(
						string.Format("({0}) IN ({1})", StringHelper.Join(", ", columnNames[tableIndex]), idSubselect));

				if (Factory.Settings.IsCommentsEnabled)
				{
					update.SetComment("bulk update");
				}
				foreach (var specification in assignmentSpecifications)
				{
					if (specification.AffectsTable(tableNames[tableIndex]))
					{
						affected = true;
						update.AppendAssignmentFragment(specification.SqlAssignmentFragment);
						if (specification.Parameters != null)
						{
							for (int paramIndex = 0; paramIndex < specification.Parameters.Length; paramIndex++)
							{
								parameterList.Add(specification.Parameters[paramIndex]);
							}
						}
					}
				}
				if (affected)
				{
					updates[tableIndex] = update.ToSqlString();
					hqlParameters[tableIndex] = parameterList.ToArray();
				}
			}
		}

		public override SqlString[] SqlStatements
		{
			get { return updates; }
		}

		public override async Task<int> Execute(QueryParameters parameters, ISessionImplementor session)
		{
			CoordinateSharedCacheCleanup(session);

			await CreateTemporaryTableIfNecessary(persister, session).ConfigureAwait(false);

			try
			{
				// First, save off the pertinent ids, as the return value
				DbCommand ps = null;
				int resultCount;
				try
				{
					try
					{
						int parameterStart = Walker.NumberOfParametersInSetClause;

						IList<IParameterSpecification> allParams = Walker.Parameters;

						List<IParameterSpecification> whereParams = (new List<IParameterSpecification>(allParams)).GetRange(
							parameterStart, allParams.Count - parameterStart);

						var sqlQueryParametersList = idInsertSelect.GetParameters().ToList();
						SqlType[] parameterTypes = whereParams.GetQueryParameterTypes(sqlQueryParametersList, session.Factory);

						ps = await session.Batcher.PrepareCommand(CommandType.Text, idInsertSelect, parameterTypes).ConfigureAwait(false);
						foreach (var parameterSpecification in whereParams)
						{
							await parameterSpecification.Bind(ps, sqlQueryParametersList, parameters, session).ConfigureAwait(false);
						}

						resultCount = await session.Batcher.ExecuteNonQuery(ps).ConfigureAwait(false);
					}
					finally
					{
						if (ps != null)
						{
							session.Batcher.CloseCommand(ps, null);
						}
					}
				}
				catch (DbException e)
				{
					throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "could not insert/select ids for bulk update",
					                                 idInsertSelect);
				}

				// Start performing the updates
				for (int i = 0; i < updates.Length; i++)
				{
					if (updates[i] == null)
					{
						continue;
					}
					try
					{
						try
						{
							var sqlQueryParametersList = updates[i].GetParameters().ToList();
							var paramsSpec = hqlParameters[i];
							SqlType[] parameterTypes = paramsSpec.GetQueryParameterTypes(sqlQueryParametersList, session.Factory);

							ps = await session.Batcher.PrepareCommand(CommandType.Text, updates[i], parameterTypes).ConfigureAwait(false);
							foreach (var parameterSpecification in paramsSpec)
							{
								await parameterSpecification.Bind(ps, sqlQueryParametersList, parameters, session).ConfigureAwait(false);
							}
							await session.Batcher.ExecuteNonQuery(ps).ConfigureAwait(false);
						}
						finally
						{
							if (ps != null)
							{
								session.Batcher.CloseCommand(ps, null);
							}
						}
					}
					catch (DbException e)
					{
						throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "error performing bulk update", updates[i]);
					}
				}

				return resultCount;
			}
			finally
			{
				await DropTemporaryTableIfNecessary(persister, session).ConfigureAwait(false);
			}
		}

		protected override IQueryable[] AffectedQueryables
		{
			get { return new[] {persister}; }
		}
	}
}