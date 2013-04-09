using ECM7.Migrator.Utils;
using Microsoft.Build.Utilities;
using NLog;
using NLog.Targets;

namespace ECM7.Migrator.MSBuild
{
	[Target("MsBuild")]
	public class MsBuildNLogTarget : TargetWithLayout
	{
		private readonly TaskLoggingHelper log;

		public MsBuildNLogTarget(TaskLoggingHelper log)
		{
			Require.IsNotNull(log, "Не задан TaskLoggingHelper для MSBuild");
			this.log = log;
		}

		protected override void Write(LogEventInfo logEvent)
		{
			var level = logEvent.Level;
			string msg = Layout.Render(logEvent);

			if (level >= LogLevel.Error)
			{
				log.LogError(msg);
			}
			else if (level >= LogLevel.Warn)
			{
				log.LogWarning(msg);
			}
			else if (level >= LogLevel.Trace)
			{
				log.LogMessage(msg);
			}
		}
	}
}
