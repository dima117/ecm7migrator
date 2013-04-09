using NLog.Config;
using NLog.Targets;

namespace ECM7.Migrator.Framework.Logging
{
	using log4net;
	using log4net.Appender;
	using log4net.Repository.Hierarchy;

	/// <summary>
	/// Логирование
	/// </summary>
	public static class MigratorLogManager
	{
		public const string LOGGER_NAME = "ecm7-migrator-logger";

		private static readonly ILog log = LogManager.GetLogger(LOGGER_NAME);

		public static ILog Log
		{
			get { return log; }
		}

		private static readonly NLog.Logger logger = NLog.LogManager.GetLogger(LOGGER_NAME);

		public static NLog.Logger Logger
		{
			get { return logger; }
		}

		public static void SetLevel(string levelName)
		{
			Logger l = Log.Logger as Logger;
			if (l != null)
			{
				l.Level = l.Hierarchy.LevelMap[levelName];
			}
		}

		public static void AddAppender(IAppender appender)
		{
			IAppender exists = FindAppender(appender.Name);
			if (exists == null)
			{
				Logger l = Log.Logger as Logger;
				
				if (l != null)
				{
					l.AddAppender(appender);
					l.Hierarchy.Configured = true;
				}
			}
		}

		private static IAppender FindAppender(string appenderName)
		{
			foreach (IAppender appender in LogManager.GetRepository().GetAppenders())
			{
				if (appender.Name == appenderName)
				{
					return appender;
				}
			}

			return null;
		}

		public static void SetNLogTarget(Target nlogTarget)
		{
			if (nlogTarget != null)
			{
				SimpleConfigurator.ConfigureForTargetLogging(nlogTarget);
			}
		}
	}
}
