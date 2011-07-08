namespace ECM7.Migrator.Framework.Logging
{
	using log4net;

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
	}
}
