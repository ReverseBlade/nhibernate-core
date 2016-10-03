﻿#if NET_4_5
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using NHibernate.AdoNet.Util;
using NHibernate.Exceptions;
using System.Threading.Tasks;

namespace NHibernate.AdoNet
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class OracleDataClientBatchingBatcher : AbstractBatcher
	{
		public override async Task AddToBatchAsync(IExpectation expectation)
		{
			bool firstOnBatch = true;
			_totalExpectedRowsAffected += expectation.ExpectedRowCount;
			string lineWithParameters = null;
			var sqlStatementLogger = Factory.Settings.SqlStatementLogger;
			if (sqlStatementLogger.IsDebugEnabled || Log.IsDebugEnabled)
			{
				lineWithParameters = sqlStatementLogger.GetCommandLineWithParameters(CurrentCommand);
				var formatStyle = sqlStatementLogger.DetermineActualStyle(FormatStyle.Basic);
				lineWithParameters = formatStyle.Formatter.Format(lineWithParameters);
				_currentBatchCommandsLog.Append("command ").Append(_countOfCommands).Append(":").AppendLine(lineWithParameters);
			}

			if (Log.IsDebugEnabled)
			{
				Log.Debug("Adding to batch:" + lineWithParameters);
			}

			if (_currentBatch == null)
			{
				// use first command as the batching command
				_currentBatch = CurrentCommand;
				_parameterValueListHashTable = new Dictionary<string, List<object>>();
				//oracle does not allow array containing all null values
				// so this Dictionary is keeping track if all values are null or not
				_parameterIsAllNullsHashTable = new Dictionary<string, bool>();
			}
			else
			{
				firstOnBatch = false;
			}

			foreach (DbParameter currentParameter in CurrentCommand.Parameters)
			{
				List<object> parameterValueList;
				if (firstOnBatch)
				{
					parameterValueList = new List<object>();
					_parameterValueListHashTable.Add(currentParameter.ParameterName, parameterValueList);
					_parameterIsAllNullsHashTable.Add(currentParameter.ParameterName, true);
				}
				else
				{
					parameterValueList = _parameterValueListHashTable[currentParameter.ParameterName];
				}

				if (currentParameter.Value != DBNull.Value)
				{
					_parameterIsAllNullsHashTable[currentParameter.ParameterName] = false;
				}

				parameterValueList.Add(currentParameter.Value);
			}

			_countOfCommands++;
			if (_countOfCommands >= _batchSize)
			{
				await (ExecuteBatchWithTimingAsync(_currentBatch));
			}
		}

		protected override async Task DoExecuteBatchAsync(DbCommand ps)
		{
			if (_currentBatch != null)
			{
				int arraySize = 0;
				_countOfCommands = 0;
				Log.Info("Executing batch");
				CheckReaders();
				await (PrepareAsync(_currentBatch));
				if (Factory.Settings.SqlStatementLogger.IsDebugEnabled)
				{
					Factory.Settings.SqlStatementLogger.LogBatchCommand(_currentBatchCommandsLog.ToString());
					_currentBatchCommandsLog = new StringBuilder().AppendLine("Batch commands:");
				}

				foreach (DbParameter currentParameter in _currentBatch.Parameters)
				{
					List<object> parameterValueArray = _parameterValueListHashTable[currentParameter.ParameterName];
					currentParameter.Value = parameterValueArray.ToArray();
					arraySize = parameterValueArray.Count;
				}

				// setting the ArrayBindCount on the OracleCommand
				// this value is not a part of the ADO.NET API.
				// It's and ODP implementation, so it is being set by reflection
				SetArrayBindCount(arraySize);
				int rowsAffected;
				try
				{
					rowsAffected = await (_currentBatch.ExecuteNonQueryAsync());
				}
				catch (DbException e)
				{
					throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, e, "could not execute batch command.");
				}

				Expectations.VerifyOutcomeBatched(_totalExpectedRowsAffected, rowsAffected);
				_totalExpectedRowsAffected = 0;
				_currentBatch = null;
				_parameterValueListHashTable = null;
			}
		}
	}
}
#endif
