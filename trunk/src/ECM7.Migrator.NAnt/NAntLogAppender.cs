namespace ECM7.Migrator.NAnt
{
	using global::NAnt.Core;
	using log4net.Appender;
	using log4net.Core;

	using Level = log4net.Core.Level;

	/// <summary>
	/// Appender для записи сообщений в лог NAnt
	/// </summary>
	public class NAntLogAppender : AppenderSkeleton
	{
		protected override bool RequiresLayout
		{
			get { return true; }
		}

		private readonly Task task;

		public NAntLogAppender(Task task)
		{
			Require.IsNotNull(task, "Не задан Task для NAnt");
			this.task = task;
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			global::NAnt.Core.Level level = ConvertEventLevel(loggingEvent.Level);
			task.Log(level, RenderLoggingEvent(loggingEvent));
		}

		/// <summary>
		/// Конвертируем уровень события log4net в уровень события nant
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		private static global::NAnt.Core.Level ConvertEventLevel(Level level)
		{
			return 
				(level >= Level.Error) ? global::NAnt.Core.Level.Error :
				(level >= Level.Warn) ? global::NAnt.Core.Level.Warning :
				(level >= Level.Info) ? global::NAnt.Core.Level.Info :
				(level >= Level.Debug) ? global::NAnt.Core.Level.Debug :
				(level >= Level.Verbose) ? global::NAnt.Core.Level.Verbose :
					global::NAnt.Core.Level.None;
		}
	}
}
