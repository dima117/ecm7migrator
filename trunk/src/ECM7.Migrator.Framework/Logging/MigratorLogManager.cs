using NLog;
using NLog.Config;
using NLog.Targets;

namespace ECM7.Migrator.Framework.Logging
{
	/// <summary>
	/// Логирование
	/// </summary>
	public static class MigratorLogManager
	{
		public const string LOGGER_NAME = "ecm7-migrator-logger";

		private static readonly Logger log = LogManager.GetLogger(LOGGER_NAME);

		public static Logger Log
		{
			get { return log; }
		}

		public static void SetNLogTarget(Target target)
		{
			if (target != null)
			{
				SimpleConfigurator.ConfigureForTargetLogging(target);
			}
		}
	}
}
