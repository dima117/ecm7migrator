namespace ECM7.Migrator.MSBuild
{
	using log4net;
	using log4net.Core;

	using System;

	using Microsoft.Build.Utilities;

	/// <summary>
	/// MSBuild task logger for the migration mediator
	/// </summary>
	public class TaskLogger : ILog
	{
		private readonly TaskLoggingHelper log;

		public TaskLogger(Task task)
		{
			Require.IsNotNull(task, "Не задан task для msbuild");
			Require.IsNotNull(task.Log, "Не задан log");
			this.log = task.Log;
		}

		#region Implementation of ILog

		public void Debug(object message)
		{
			log.LogMessage(message.ToString());
		}

		public void Debug(object message, Exception exception)
		{
			log.LogMessage(message.ToString());
			log.LogErrorFromException(exception);
		}

		public void DebugFormat(string format, params object[] args)
		{
			log.LogMessage(format.FormatWith(args));
		}

		public void DebugFormat(IFormatProvider provider, string format, params object[] args)
		{
			log.LogMessage(format.FormatWith(provider, args));
		}

		public void Info(object message)
		{
			log.LogMessage(message.ToString());
		}

		public void Info(object message, Exception exception)
		{
			log.LogMessage(message.ToString());
			log.LogErrorFromException(exception);
		}

		public void InfoFormat(string format, params object[] args)
		{
			log.LogMessage(format.FormatWith(args));
		}

		public void InfoFormat(IFormatProvider provider, string format, params object[] args)
		{
			log.LogMessage(format.FormatWith(provider, args));
		}

		public void Warn(object message)
		{
			log.LogWarning(message.ToString());
		}

		public void Warn(object message, Exception exception)
		{
			log.LogWarning(message.ToString());
			log.LogWarningFromException(exception);
		}

		public void WarnFormat(string format, params object[] args)
		{
			log.LogWarning(format.FormatWith(args));
		}

		public void WarnFormat(IFormatProvider provider, string format, params object[] args)
		{
			log.LogWarning(format.FormatWith(provider, args));
		}

		public void Error(object message)
		{
			log.LogError(message.ToString());
		}

		public void Error(object message, Exception exception)
		{
			log.LogError(message.ToString());
			log.LogErrorFromException(exception);
		}

		public void ErrorFormat(string format, params object[] args)
		{
			log.LogError(format.FormatWith(args));
		}

		public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
		{
			log.LogError(format.FormatWith(provider, args));
		}

		public void Fatal(object message)
		{
			log.LogError(message.ToString());
		}

		public void Fatal(object message, Exception exception)
		{
			log.LogError(message.ToString());
			log.LogErrorFromException(exception);
		}

		public void FatalFormat(string format, params object[] args)
		{
			log.LogError(format.FormatWith(args));
		}

		public void FatalFormat(IFormatProvider provider, string format, params object[] args)
		{
			log.LogError(format.FormatWith(provider, args));
		}

		public bool IsDebugEnabled
		{
			get { return true; }
		}

		public bool IsInfoEnabled
		{
			get { return true; }
		}

		public bool IsWarnEnabled
		{
			get { return true; }
		}

		public bool IsErrorEnabled
		{
			get { return true; }
		}

		public bool IsFatalEnabled
		{
			get { return true; }
		}

		#endregion

		#region Implementation of ILoggerWrapper

		public ILogger Logger
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
