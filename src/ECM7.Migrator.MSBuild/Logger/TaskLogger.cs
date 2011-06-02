#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ECM7.Migrator.MSBuild.Logger
{
	/// <summary>
	/// MSBuild task logger for the migration mediator
	/// </summary>
	public class TaskLogger : Framework.ILogger
	{
		private readonly Task task;

		public TaskLogger(Task task)
		{
			this.task = task;
		}

		protected void LogInfo(string format, params object[] args)
		{
			this.task.Log.LogMessage(format, args);
		}

		protected void LogError(string format, params object[] args)
		{
			this.task.Log.LogError(format, args);
		}

		public void Started(long currentVersion, long finalVersion)
		{
			LogInfo("Current version : {0}", currentVersion);
		}

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
			this.task.Log.LogErrorFromException(ex, true);
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
			this.task.Log.LogErrorFromException(ex, true);
			LogInfo("======================================");
		}

		public void Finished(long originalVersion, long currentVersion)
		{
			LogInfo("Migrated to version {0}", currentVersion);
		}

		/// <summary>
		/// Log that we have finished a migration
		/// </summary>
		/// <param name="originalVersion">List of versions with which we started</param>
		/// <param name="currentVersion">Final Version</param>
		public void Finished(IList<long> originalVersion, long currentVersion)
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
			this.task.Log.LogWarning("[Warning] {0}", String.Format(format, args));
		}

		/// <summary>
		/// Log a Trace Message
		/// </summary>
		/// <param name="format">The format string ("{0}, blabla {1}").</param>
		/// <param name="args">Parameters to apply to the format string.</param>
		public void Trace(string format, params object[] args)
		{
			this.task.Log.LogMessage(MessageImportance.Low, format, args);
		}

		private string LatestVersion(IList<long> versions)
		{
			if (versions.Count > 0)
			{
				return versions[versions.Count - 1].ToString();
			}

			return "No migrations applied yet!";
		}
	}
}
