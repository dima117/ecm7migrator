using System;
using System.Collections.Generic;
using System.IO;

namespace ECM7.Migrator.Framework.Loggers
{
    public class SqlScriptFileLogger : ILogger, IDisposable
    {
        private readonly ILogger innerLogger;
        private TextWriter streamWriter;

        public SqlScriptFileLogger(ILogger logger, TextWriter streamWriter)
        {
            innerLogger = logger;
            this.streamWriter = streamWriter;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (streamWriter != null)
            {
                streamWriter.Dispose();
                streamWriter = null;
            }
        }

        #endregion
        
		#region ILogger members

		public void Log(string format, params object[] args)
		{
			innerLogger.Log(format, args);
		}

		public void Warn(string format, params object[] args)
		{
			innerLogger.Warn(format, args);
		}

		public void Trace(string format, params object[] args)
		{
			innerLogger.Trace(format, args);
		}

		public void ApplyingDatabaseChange(string sql)
		{
			innerLogger.ApplyingDatabaseChange(sql);
			streamWriter.WriteLine(sql);
		}

		public void Started(List<long> appliedVersions, long finalVersion)
		{
			innerLogger.Started(appliedVersions, finalVersion);
		}

		public void MigrateUp(long version, string migrationName)
		{
			innerLogger.MigrateUp(version, migrationName);
		}

		public void MigrateDown(long version, string migrationName)
		{
			innerLogger.MigrateDown(version, migrationName);
		}

		public void Skipping(long version)
		{
			innerLogger.Skipping(version);
		}

		public void RollingBack(long originalVersion)
		{
			innerLogger.RollingBack(originalVersion);
		}

		public void Exception(long version, string migrationName, Exception ex)
		{
			innerLogger.Exception(version, migrationName, ex);
		}

		public void Exception(string message, Exception ex)
		{
			innerLogger.Exception(message, ex);
		}

		public void Finished(List<long> appliedVersions, long currentVersion)
		{
			innerLogger.Finished(appliedVersions, currentVersion);
			streamWriter.Close();
		} 
		
		#endregion
    }
}
