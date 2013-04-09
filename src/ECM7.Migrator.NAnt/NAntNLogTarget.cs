using ECM7.Migrator.Utils;
using NAnt.Core;
using NLog;
using NLog.Targets;

namespace ECM7.Migrator.NAnt
{
	[Target("NAnt")]
	public class NAntNLogTarget : TargetWithLayout
	{
		private readonly Task task;

		public NAntNLogTarget(Task task)
		{
			Require.IsNotNull(task, "Не задан Task для NAnt");
			this.task = task;
		}

		protected override void Write(LogEventInfo logEvent)
		{
			Level level = ConvertEventLevel(logEvent.Level);
			string msg = Layout.Render(logEvent);
			task.Log(level, msg);
		}

		/// <summary>
		/// Конвертируем уровень события NLog в уровень события nant
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		private static Level ConvertEventLevel(LogLevel level)
		{
			return
				(level >= LogLevel.Error) ? Level.Error :
				(level >= LogLevel.Warn)  ? Level.Warning :
				(level >= LogLevel.Info)  ? Level.Info :
				(level >= LogLevel.Debug) ? Level.Debug :
				(level >= LogLevel.Trace) ? Level.Verbose : Level.None;
		}
	}
}
