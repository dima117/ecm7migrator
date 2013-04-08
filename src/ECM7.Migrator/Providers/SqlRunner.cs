using ECM7.Migrator.Utils;

namespace ECM7.Migrator.Providers
{
	using System;
	using System.Data;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using Exceptions;
	using Framework.Logging;

	public class SqlRunner : ContextBoundObject, IDisposable
	{
		protected SqlRunner(IDbConnection connection, int commandTimeout)
		{
			Require.IsNotNull(connection, "Не инициализировано подключение к БД");
			this.connection = connection;
			this.commandTimeout = commandTimeout;
		}

		private bool connectionNeedClose; // = false

		private readonly IDbConnection connection;

		private readonly int commandTimeout;

		private IDbTransaction transaction;

		public int? CommandTimeout
		{
			get { return commandTimeout; }
		}

		public IDbConnection Connection
		{
			get { return connection; }
		}

		public virtual string BatchSeparator
		{
			get { return null; }
		}

		#region public methods

		public IDataReader ExecuteReader(string sql)
		{
			IDbCommand cmd = null;
			IDataReader reader = null;

			try
			{
				MigratorLogManager.Log.ExecuteSql(sql);
				cmd = GetCommand(sql);
				reader = OpenDataReader(cmd);
				return reader;
			}
			catch (Exception ex)
			{
				if (reader != null)
				{
					reader.Dispose();
				}

				if (cmd != null)
				{
					MigratorLogManager.Log.WarnFormat("query failed: {0}", cmd.CommandText);
					cmd.Dispose();
				}

				throw new SQLException(ex);
			}
		}

		public object ExecuteScalar(string sql)
		{
			using (IDbCommand cmd = GetCommand(sql))
			{
				try
				{
					MigratorLogManager.Log.ExecuteSql(sql);
					return cmd.ExecuteScalar();
				}
				catch (Exception ex)
				{
					MigratorLogManager.Log.WarnFormat("Query failed: {0}", cmd.CommandText);
					throw new SQLException(ex);
				}
			}
		}

		public int ExecuteNonQuery(string sql)
		{
			int result = 0;

			try
			{
				// если задан разделитель пакетов запросов, запускаем пакеты по очереди
				if (!BatchSeparator.IsNullOrEmpty(true) &&
					sql.IndexOf(BatchSeparator, StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					sql += "\n" + BatchSeparator.Trim(); // make sure last batch is executed.

					string[] lines = sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

					var sqlBatch = new StringBuilder();

					foreach (string line in lines)
					{
						if (line.ToUpperInvariant().Trim() == BatchSeparator.ToUpperInvariant())
						{
							string query = sqlBatch.ToString();
							if (!query.IsNullOrEmpty(true))
							{
								result = ExecuteNonQueryInternal(query);
							}

							sqlBatch.Clear();
						}
						else
						{
							sqlBatch.AppendLine(line.Trim());
						}
					}
				}
				else
				{
					result = ExecuteNonQueryInternal(sql);
				}
			}
			catch (Exception ex)
			{
				MigratorLogManager.Log.Warn(ex.Message, ex);
				throw new SQLException(ex);
			}

			return result;
		}

		public void ExecuteFromResource(Assembly assembly, string path)
		{
			Require.IsNotNull(assembly, "Incorrect assembly");

			using (Stream stream = assembly.GetManifestResourceStream(path))
			{
				Require.IsNotNull(stream, "Не удалось загрузить указанный файл ресурсов");

				// ReSharper disable AssignNullToNotNullAttribute
				using (var reader = new StreamReader(stream))
				{
					string sql = reader.ReadToEnd();
					ExecuteNonQuery(sql);
				}
				// ReSharper restore AssignNullToNotNullAttribute
			}
		}


		#endregion

		#region transactions

		/// <summary>
		/// Starts a transaction. Called by the migration mediator.
		/// </summary>
		public void BeginTransaction()
		{
			if (transaction == null && connection != null)
			{
				EnsureHasConnection();
				transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
			}
		}

		/// <summary>
		/// Commit the current transaction. Called by the migrations mediator.
		/// </summary>
		public void Commit()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Commit();
				}
				catch (Exception ex)
				{
					MigratorLogManager.Log.Error("Не удалось применить транзакцию", ex);
				}
			}
			transaction = null;
		}

		/// <summary>
		/// Rollback the current migration. Called by the migration mediator.
		/// </summary>
		public virtual void Rollback()
		{
			if (transaction != null && connection != null && connection.State == ConnectionState.Open)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception ex)
				{
					MigratorLogManager.Log.Error("Не удалось откатить транзакцию", ex);
				}
			}
			transaction = null;
		}

		#endregion		

		#region helpers

		protected void EnsureHasConnection()
		{
			if (connection.State != ConnectionState.Open)
			{
				connectionNeedClose = true;
				connection.Open();
			}
		}

		private int ExecuteNonQueryInternal(string sql)
		{
			MigratorLogManager.Log.ExecuteSql(sql);
			using (IDbCommand cmd = GetCommand(sql))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public virtual IDbCommand GetCommand(string sql = null)
		{
			EnsureHasConnection();

			IDbCommand cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.CommandType = CommandType.Text;

			if (commandTimeout > 0)
			{
				cmd.CommandTimeout = commandTimeout;
			}

			if (transaction != null)
			{
				cmd.Transaction = transaction;
			}
			return cmd;
		}

		protected virtual IDataReader OpenDataReader(IDbCommand cmd)
		{
			return cmd.ExecuteReader();
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (connectionNeedClose && connection != null && connection.State == ConnectionState.Open)
			{
				connection.Close();
				connectionNeedClose = false;
			}
		}

		#endregion
	}
}
