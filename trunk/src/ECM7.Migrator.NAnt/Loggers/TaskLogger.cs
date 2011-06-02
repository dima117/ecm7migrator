namespace ECM7.Migrator.NAnt.Loggers
{
	using System;
	using System.Collections.Generic;
	using ECM7.Migrator.Framework;
	using global::NAnt.Core;

	/// <summary>
	/// NAnt task logger for the migration mediator
	/// </summary>
	public class TaskLogger : ILogger
	{
		/// <summary>
		/// Таск NAnt, который работает с логгером
		/// </summary>
		private readonly Task task;

		/// <summary>
		/// Инициализация
		/// </summary>
		/// <param name="task">Таск NAnt, который работает с логгером</param>
		public TaskLogger(Task task)
		{
			this.task = task;
		}

		/// <summary>
		/// Записать в лог информационное сообщение
		/// </summary>
		/// <param name="format">Текст сообщения</param>
		/// <param name="args">Параметры сообщения</param>
		protected void LogInfo(string format, params object[] args)
		{
			task.Log(Level.Info, format, args);
		}

		/// <summary>
		/// Записать в лог сообщение об ошибке
		/// </summary>
		/// <param name="format">Текст сообщения</param>
		/// <param name="args">Параметры сообщения</param>
		protected void LogError(string format, params object[] args)
		{
			task.Log(Level.Error, format, args);
		}

		/// <summary>
		/// Записать в лог сообщение о начале выполнения миграций
		/// </summary>
		/// <param name="currentVersion">Текущая версия</param>
		/// <param name="finalVersion">Целевая версия</param>
		public void Started(long currentVersion, long finalVersion)
		{
			LogInfo("Current version : {0}", currentVersion);
		}

		/// <summary>
		/// Записать в лог подробную информацию об исключении
		/// </summary>
		/// <param name="ex">Исключение</param>
		private void LogExceptionDetails(Exception ex)
		{
			LogInfo("{0}", ex.Message);
			LogInfo("{0}", ex.StackTrace);
			Exception iex = ex.InnerException;
			while (iex != null)
			{
				LogInfo("Caused by: {0}", iex);
				LogInfo("{0}", ex.StackTrace);
				iex = iex.InnerException;
			}
		}

		/// <summary>
		/// Записать в лог сообщение о завершении выполнения миграций
		/// </summary>
		/// <param name="originalVersion">Начальная версия БД</param>
		/// <param name="currentVersion">Текущая версия БД</param>
		public void Finished(long originalVersion, long currentVersion)
		{
			LogInfo("Migrated to version {0}", currentVersion);
		}

		/// <summary>
		/// Получить номер последней версии
		/// </summary>
		/// <param name="versions">Список номеров версий</param>
		private static string LatestVersion(IList<long> versions)
		{
			if (versions.Count > 0)
			{
				return versions[versions.Count - 1].ToString();
			}

			return "No migrations applied yet!";
		}

		#region ILogger members

		/// <summary>
		/// Log that we have started a migration
		/// </summary>
		/// <param name="currentVersions">Start list of versions</param>
		/// <param name="finalVersion">Final Version</param>
		public void Started(IList<long> currentVersions, long finalVersion)
		{
			LogInfo("Latest version applied : {0}.  Target version : {1}", LatestVersion(currentVersions), finalVersion);
		}

		/// <summary>
		/// Log that we are migrating up
		/// </summary>
		/// <param name="version">Version we are migrating to</param>
		/// <param name="migrationName">Migration name</param>
		public void MigrateUp(long version, string migrationName)
		{
			LogInfo("Applying {0}: {1}", version.ToString(), migrationName);
		}

		/// <summary>
		/// Log that we are migrating down
		/// </summary>
		/// <param name="version">Version we are migrating to</param>
		/// <param name="migrationName">Migration name</param>
		public void MigrateDown(long version, string migrationName)
		{
			LogInfo("Removing {0}: {1}", version.ToString(), migrationName);
		}

		/// <summary>
		/// Inform that a migration corresponding to the number of
		/// version is untraceable (not found?) and will be ignored.
		/// </summary>
		/// <param name="version">Version we couldnt find</param>
		public void Skipping(long version)
		{
			MigrateUp(version, "<Migration not found>");
		}

		/// <summary>
		/// Log that we are rolling back to version
		/// </summary>
		/// <param name="originalVersion">
		/// version
		/// </param>
		public void RollingBack(long originalVersion)
		{
			LogInfo("Rolling back to migration {0}", originalVersion);
		}

		/// <summary>
		/// Log a Sql statement that changes the schema or content of the database as part of a migration
		/// </summary>
		/// <remarks>
		/// SELECT statements should not be logged using this method as they do not alter the data or schema of the
		/// database.
		/// </remarks>
		/// <param name="sql">The Sql statement to log</param>
		public void ApplyingDatabaseChange(string sql)
		{
			Log(sql);
		}

		/// <summary>
		/// Log that we had an exception on a migration
		/// </summary>
		/// <param name="version">The version of the migration that caused the exception.</param>
		/// <param name="migrationName">The name of the migration that caused the exception.</param>
		/// <param name="ex">The exception itself</param>
		public void Exception(long version, string migrationName, Exception ex)
		{
			LogInfo("============ Error Detail ============");
			LogInfo("Error in migration: {0}", version);
			LogExceptionDetails(ex);
			LogInfo("======================================");
		}

		/// <summary>
		/// Log that we had an exception on a migration
		/// </summary>
		/// <param name="message">An informative message to show to the user.</param>
		/// <param name="ex">The exception itself</param>
		public void Exception(string message, Exception ex)
		{
			LogInfo("============ Error Detail ============");
			LogInfo("Error: {0}", message);
			LogExceptionDetails(ex);
			LogInfo("======================================");
		}

		/// <summary>
		/// Log that we have finished a migration
		/// </summary>
		/// <param name="originalVersions">List of versions with which we started</param>
		/// <param name="currentVersion">Final Version</param>
		public void Finished(IList<long> originalVersions, long currentVersion)
		{
			LogInfo("Migrated to version {0}", currentVersion);
		}

		/// <summary>
		/// Log a message
		/// </summary>
		/// <param name="format">The format string ("{0}, blabla {1}").</param>
		/// <param name="args">Parameters to apply to the format string.</param>
		public void Log(string format, params object[] args)
		{
			LogInfo(format, args);
		}

		/// <summary>
		/// Log a Warning
		/// </summary>
		/// <param name="format">The format string ("{0}, blabla {1}").</param>
		/// <param name="args">Parameters to apply to the format string.</param>
		public void Warn(string format, params object[] args)
		{
			LogInfo("[Warning] {0}", String.Format(format, args));
		}

		/// <summary>
		/// Log a Trace Message
		/// </summary>
		/// <param name="format">The format string ("{0}, blabla {1}").</param>
		/// <param name="args">Parameters to apply to the format string.</param>
		public void Trace(string format, params object[] args)
		{
			task.Log(Level.Debug, format, args);
		}

		#endregion
	}
}
