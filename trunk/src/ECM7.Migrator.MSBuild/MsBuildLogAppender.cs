using ECM7.Migrator.Utils;

namespace ECM7.Migrator.MSBuild
{
	using log4net.Appender;
	using log4net.Core;

	using Microsoft.Build.Utilities;

	/// <summary>
	/// Appender для записи сообщений в лог NAnt
	/// </summary>
	public class MSBuildLogAppender : AppenderSkeleton
	{
		protected override bool RequiresLayout
		{
			get { return true; }
		}

		private readonly TaskLoggingHelper log;

		public MSBuildLogAppender(TaskLoggingHelper log)
		{
			Require.IsNotNull(log, "Не задан TaskLoggingHelper для MSBuild");
			this.log = log;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			var level = loggingEvent.Level;
			string msg = RenderLoggingEvent(loggingEvent);

			if (level >= Level.Error)
			{
				log.LogError(msg);
			}
			else if (level >= Level.Warn)
			{
				log.LogWarning(msg);
			}
			else if (level >= Level.Verbose)
			{
				log.LogMessage(msg);
			}
		}
	}
}
